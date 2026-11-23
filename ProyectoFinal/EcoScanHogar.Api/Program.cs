using EcoScanHogar.Api.Data;
using EcoScanHogar.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EcoScanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EcoScan") ?? "Data Source=ecoscan.db"));

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EcoScanDbContext>();
    await db.Database.EnsureCreatedAsync();
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE Products ADD COLUMN IsWasted INTEGER NOT NULL DEFAULT 0"); }
    catch { /* The column already exists. */ }
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS Users (Id TEXT NOT NULL PRIMARY KEY, Name TEXT NOT NULL, Email TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, PasswordSalt TEXT NOT NULL)");
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapPost("/api/auth/register", async (LoginRequest request, EcoScanDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 4)
        return Results.BadRequest(new { message = "Completa los datos y usa una contraseña de al menos 4 caracteres." });
    if (await db.Users.AnyAsync(x => x.Email == request.Email.Trim().ToLowerInvariant())) return Results.Conflict(new { message = "Este correo ya está registrado." });
    var salt = RandomNumberGenerator.GetBytes(16);
    var hash = Rfc2898DeriveBytes.Pbkdf2(request.Password, salt, 100000, HashAlgorithmName.SHA256, 32);
    var user = new UserEntity { Name = request.Name.Trim(), Email = request.Email.Trim().ToLowerInvariant(), PasswordSalt = Convert.ToBase64String(salt), PasswordHash = Convert.ToBase64String(hash) };
    db.Users.Add(user); await db.SaveChangesAsync(); return Results.Ok(new { user.Name, user.Email });
});
app.MapPost("/api/auth/login", async (LoginRequest request, EcoScanDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant());
    if (user is null) return Results.Unauthorized();
    var hash = Rfc2898DeriveBytes.Pbkdf2(request.Password, Convert.FromBase64String(user.PasswordSalt), 100000, HashAlgorithmName.SHA256, 32);
    return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(user.PasswordHash)) ? Results.Ok(new { user.Name, user.Email }) : Results.Unauthorized();
});
app.MapGet("/api/products", async (EcoScanDbContext db) =>
    await db.Products.OrderBy(x => x.ExpirationDate).ToListAsync());
app.MapPost("/api/products", async (ProductEntity product, EcoScanDbContext db) =>
{
    product.Id = Guid.NewGuid();
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
});
app.MapPut("/api/products/{id:guid}/consume", async (Guid id, EcoScanDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    product.IsConsumed = true;
    product.IsWasted = false;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapPut("/api/products/{id:guid}/waste", async (Guid id, EcoScanDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    product.IsWasted = true;
    product.IsConsumed = false;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/api/products/{id:guid}", async (Guid id, EcoScanDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

record LoginRequest(string Name, string Email, string Password);
