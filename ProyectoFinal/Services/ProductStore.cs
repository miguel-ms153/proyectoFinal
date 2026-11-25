using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Networking;
using ProyectoFinal.Models;
using SQLite;

namespace ProyectoFinal.Services;

public sealed class ProductStore
{
    private static readonly HttpClient Client = new()
    {
        BaseAddress = new Uri("http://10.0.2.2:5188/"),
        Timeout = TimeSpan.FromSeconds(6)
    };
    private static readonly SQLiteAsyncConnection Database = new(
        Path.Combine(FileSystem.AppDataDirectory, "ecoscan-offline.db3"),
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
    private static readonly SemaphoreSlim DatabaseLock = new(1, 1);
    private static readonly SemaphoreSlim SynchronizationLock = new(1, 1);
    private static bool isInitialized;
    private static bool isMonitoringConnection;

    public string SyncStatus { get; private set; } = "Comprobando conexión…";

    public ProductStore()
    {
        RegisterConnectivityMonitoring();
    }

    public async Task<List<ProductItem>> GetAsync()
    {
        var result = await SyncAsync();
        SyncStatus = result.Message;
        return await GetCachedProductsAsync();
    }

    public async Task SaveAsync(ProductItem product)
    {
        await InitializeAsync();
        await Database.InsertOrReplaceAsync(CachedProduct.From(product));
        if (!await SendProductAsync(product))
            await QueueAsync("create", product);
    }

    public async Task ConsumeAsync(Guid id)
    {
        var product = await FindCachedProductAsync(id);
        if (product is null) return;
        product.IsConsumed = true;
        product.IsWasted = false;
        await Database.InsertOrReplaceAsync(CachedProduct.From(product));
        if (!await SendActionAsync(id, "consume"))
            await QueueAsync("consume", product);
    }

    public async Task WasteAsync(Guid id)
    {
        var product = await FindCachedProductAsync(id);
        if (product is null) return;
        product.IsWasted = true;
        product.IsConsumed = false;
        await Database.InsertOrReplaceAsync(CachedProduct.From(product));
        if (!await SendActionAsync(id, "waste"))
            await QueueAsync("waste", product);
    }

    public async Task<SyncResult> SyncAsync()
    {
        await InitializeAsync();
        if (!await SynchronizationLock.WaitAsync(0))
            return new SyncResult(false, await PendingCountAsync(), "Sincronización en curso…");

        try
        {
            var pending = await Database.Table<PendingSyncOperation>().OrderBy(x => x.CreatedAtUtc).ToListAsync();
            foreach (var operation in pending)
            {
                var completed = operation.Action switch
                {
                    "create" => await SendProductAsync(JsonSerializer.Deserialize<ProductItem>(operation.ProductJson)),
                    "consume" => await SendActionAsync(Guid.Parse(operation.ProductId), "consume"),
                    "waste" => await SendActionAsync(Guid.Parse(operation.ProductId), "waste"),
                    _ => true
                };
                if (!completed)
                    return new SyncResult(false, pending.Count, $"Sin conexión · {pending.Count} cambio(s) pendiente(s)");
                await Database.DeleteAsync(operation);
            }

            var remote = await Client.GetFromJsonAsync<List<ProductItem>>("api/products");
            if (remote is not null)
            {
                await Database.DeleteAllAsync<CachedProduct>();
                if (remote.Count > 0)
                    await Database.InsertAllAsync(remote.Select(CachedProduct.From));
            }
            var remaining = await PendingCountAsync();
            return new SyncResult(true, remaining, remaining == 0 ? "Sincronizado con MySQL" : $"{remaining} cambio(s) pendiente(s)");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            var pending = await PendingCountAsync();
            return new SyncResult(false, pending, pending == 0 ? "Modo sin conexión" : $"Sin conexión · {pending} cambio(s) pendiente(s)");
        }
        finally
        {
            SynchronizationLock.Release();
        }
    }

    private static async Task<bool> SendProductAsync(ProductItem? product)
    {
        if (product is null) return true;
        try { return (await Client.PostAsJsonAsync("api/products", product)).IsSuccessStatusCode; }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return false; }
    }

    private static async Task<bool> SendActionAsync(Guid id, string action)
    {
        try { return (await Client.PutAsync($"api/products/{id}/{action}", null)).IsSuccessStatusCode; }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return false; }
    }

    private static async Task QueueAsync(string action, ProductItem product)
    {
        await InitializeAsync();
        await Database.InsertAsync(new PendingSyncOperation
        {
            Action = action,
            ProductId = product.Id.ToString(),
            ProductJson = JsonSerializer.Serialize(product)
        });
    }

    private static async Task<ProductItem?> FindCachedProductAsync(Guid id)
    {
        await InitializeAsync();
        var cached = await Database.FindAsync<CachedProduct>(id.ToString());
        return cached?.ToProduct();
    }

    private static async Task<List<ProductItem>> GetCachedProductsAsync()
    {
        await InitializeAsync();
        var items = await Database.Table<CachedProduct>().ToListAsync();
        return items.Select(x => x.ToProduct()).OrderBy(x => x.ExpirationDate).ToList();
    }

    private static async Task<int> PendingCountAsync()
    {
        await InitializeAsync();
        return await Database.Table<PendingSyncOperation>().CountAsync();
    }

    private static async Task InitializeAsync()
    {
        if (isInitialized) return;
        await DatabaseLock.WaitAsync();
        try
        {
            if (isInitialized) return;
            await Database.CreateTableAsync<CachedProduct>();
            await Database.CreateTableAsync<PendingSyncOperation>();
            isInitialized = true;
        }
        finally { DatabaseLock.Release(); }
    }

    private static void RegisterConnectivityMonitoring()
    {
        if (isMonitoringConnection) return;
        isMonitoringConnection = true;
        Connectivity.Current.ConnectivityChanged += async (_, e) =>
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
                await new ProductStore().SyncAsync();
        };
    }
}
