using System.Net.Http.Json;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services;

// Replace this address with the deployed service URL for Android devices.
public sealed class ProductApiClient(HttpClient client)
{
    public async Task<IReadOnlyList<ProductItem>> GetProductsAsync(CancellationToken token = default) =>
        await client.GetFromJsonAsync<List<ProductItem>>("api/products", token) ?? [];
    public async Task<bool> IsServiceAvailableAsync(CancellationToken token = default)
    {
        try { return (await client.GetAsync("api/health", token)).IsSuccessStatusCode; }
        catch (HttpRequestException) { return false; }
    }
}
