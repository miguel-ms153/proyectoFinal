using EcoScanHogar.Api.Data;
using EcoScanHogar.Api.Models;
using EcoScanHogar.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("EcoScan")
    ?? throw new InvalidOperationException("Falta la cadena de conexión EcoScan.");
builder.Services.AddDbContext<EcoScanDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddSingleton<GmailEmailService>();

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EcoScanDbContext>();
    await db.Database.EnsureCreatedAsync();
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE Products ADD COLUMN IsWasted TINYINT(1) NOT NULL DEFAULT 0"); }
    catch { /* La columna ya existe. */ }
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", database = "MySQL" }));
app.MapPost("/api/auth/register", async (LoginRequest request, EcoScanDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 4)
        return Results.BadRequest(new { message = "Completa los datos y usa una contraseña de al menos 4 caracteres." });
    var email = request.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(x => x.Email == email)) return Results.Conflict(new { message = "Este correo ya está registrado." });
    var user = new UserEntity { Name = request.Name.Trim(), Email = email };
    SetPassword(user, request.Password);
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { user.Name, user.Email });
});
app.MapPost("/api/auth/login", async (LoginRequest request, EcoScanDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant());
    if (user is null) return Results.Unauthorized();
    var hash = Rfc2898DeriveBytes.Pbkdf2(request.Password, Convert.FromBase64String(user.PasswordSalt), 100000, HashAlgorithmName.SHA256, 32);
    return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(user.PasswordHash)) ? Results.Ok(new { user.Name, user.Email }) : Results.Unauthorized();
});
app.MapPost("/api/auth/password-reset/request", async (PasswordResetRequest request, EcoScanDbContext db, GmailEmailService emailService) =>
{
    var email = request.Email?.Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(email)) return Results.BadRequest();
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
    if (user is null) return Results.Ok(new { message = "Si existe una cuenta, se envió el código." });
    var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    db.PasswordResetCodes.RemoveRange(db.PasswordResetCodes.Where(x => x.Email == email));
    db.PasswordResetCodes.Add(new PasswordResetEntity { Email = email, CodeHash = HashCode(code), ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15) });
    await db.SaveChangesAsync();
    try { await emailService.SendResetCodeAsync(email, code); return Results.Ok(new { message = "Código enviado." }); }
    catch (InvalidOperationException ex) { app.Logger.LogWarning(ex, "La configuración de Gmail no está disponible."); return Results.Problem("El correo de recuperación no está configurado.", statusCode: 503); }
    catch (Exception ex) { app.Logger.LogError(ex, "No se pudo enviar recuperación a {Recipient}.", email); return Results.Problem("No se pudo enviar el correo.", statusCode: 502); }
});
app.MapPost("/api/auth/password-reset/confirm", async (PasswordResetConfirm request, EcoScanDbContext db) =>
{
    var email = request.Email?.Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Code) || request.NewPassword.Length < 4) return Results.BadRequest();
    var codeHash = HashCode(request.Code.Trim());
    var reset = await db.PasswordResetCodes.FirstOrDefaultAsync(x => x.Email == email && x.ExpiresAtUtc > DateTime.UtcNow && x.CodeHash == codeHash);
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
    if (reset is null || user is null) return Results.BadRequest();
    SetPassword(user, request.NewPassword);
    db.PasswordResetCodes.Remove(reset);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Contraseña actualizada." });
});
app.MapGet("/api/products", async (EcoScanDbContext db) => await db.Products.OrderBy(x => x.ExpirationDate).ToListAsync());
app.MapPost("/api/products", async (ProductEntity product, EcoScanDbContext db) =>
{
    if (product.Id == Guid.Empty) product.Id = Guid.NewGuid();
    var existing = await db.Products.FindAsync(product.Id);
    if (existing is null) db.Products.Add(product);
    else
    {
        existing.Name = product.Name;
        existing.Category = product.Category;
        existing.Quantity = product.Quantity;
        existing.ExpirationDate = product.ExpirationDate;
        existing.IsConsumed = product.IsConsumed;
        existing.IsWasted = product.IsWasted;
    }
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
});
app.MapPut("/api/products/{id:guid}/consume", async (Guid id, EcoScanDbContext db) => { var product = await db.Products.FindAsync(id); if (product is null) return Results.NotFound(); product.IsConsumed = true; product.IsWasted = false; await db.SaveChangesAsync(); return Results.NoContent(); });
app.MapPut("/api/products/{id:guid}/waste", async (Guid id, EcoScanDbContext db) => { var product = await db.Products.FindAsync(id); if (product is null) return Results.NotFound(); product.IsWasted = true; product.IsConsumed = false; await db.SaveChangesAsync(); return Results.NoContent(); });
app.MapDelete("/api/products/{id:guid}", async (Guid id, EcoScanDbContext db) => { var product = await db.Products.FindAsync(id); if (product is null) return Results.NotFound(); db.Products.Remove(product); await db.SaveChangesAsync(); return Results.NoContent(); });
app.Run();

static void SetPassword(UserEntity user, string password)
{
    var salt = RandomNumberGenerator.GetBytes(16);
    user.PasswordSalt = Convert.ToBase64String(salt);
    user.PasswordHash = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32));
}
static string HashCode(string code) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

record LoginRequest(string Name, string Email, string Password);
record PasswordResetRequest(string? Email);
record PasswordResetConfirm(string? Email, string? Code, string NewPassword);
