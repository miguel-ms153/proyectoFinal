using EcoScanHogar.Api.Data;
using EcoScanHogar.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EcoScanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EcoScan") ?? "Data Source=ecoscan.db"));

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EcoScanDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
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
