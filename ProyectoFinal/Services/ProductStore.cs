using System.Net.Http.Json;
using System.Text.Json;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services;

public sealed class ProductStore
{
    private static readonly HttpClient Client = new() { BaseAddress = new Uri("http://10.0.2.2:5188/") };
    private static readonly string PathName = Path.Combine(FileSystem.AppDataDirectory, "ecoscan-products.json");
    public async Task<List<ProductItem>> GetAsync()
    {
        try { var remote = await Client.GetFromJsonAsync<List<ProductItem>>("api/products"); if (remote is not null) return remote; } catch (HttpRequestException) { }
        return await GetLocalAsync();
    }
    public async Task SaveAsync(ProductItem product)
    {
        try { if ((await Client.PostAsJsonAsync("api/products", product)).IsSuccessStatusCode) return; } catch (HttpRequestException) { }
        var items = await GetLocalAsync(); items.Add(product); await WriteAsync(items);
    }
    public async Task ConsumeAsync(Guid id)
    {
        try { if ((await Client.PutAsync($"api/products/{id}/consume", null)).IsSuccessStatusCode) return; } catch (HttpRequestException) { }
        var items = await GetLocalAsync(); var product = items.FirstOrDefault(x => x.Id == id); if (product is not null) { product.IsConsumed = true; product.IsWasted = false; } await WriteAsync(items);
    }
    public async Task WasteAsync(Guid id)
    {
        try { if ((await Client.PutAsync($"api/products/{id}/waste", null)).IsSuccessStatusCode) return; } catch (HttpRequestException) { }
        var items = await GetLocalAsync(); var product = items.FirstOrDefault(x => x.Id == id); if (product is not null) { product.IsWasted = true; product.IsConsumed = false; } await WriteAsync(items);
    }
    private static async Task<List<ProductItem>> GetLocalAsync() { if (!File.Exists(PathName)) return Seed(); await using var stream = File.OpenRead(PathName); return await JsonSerializer.DeserializeAsync<List<ProductItem>>(stream) ?? []; }
    private static async Task WriteAsync(List<ProductItem> items) { await using var stream = File.Create(PathName); await JsonSerializer.SerializeAsync(stream, items); }
    private static List<ProductItem> Seed() => [new() { Name = "Leche", Category = "Lácteos", ExpirationDate = DateTime.Today.AddDays(2) }, new() { Name = "Arroz", Category = "Despensa", Quantity = 2, ExpirationDate = DateTime.Today.AddDays(45) }];
}
