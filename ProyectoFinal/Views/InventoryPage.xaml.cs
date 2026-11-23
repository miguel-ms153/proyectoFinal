using ProyectoFinal.Services;
namespace ProyectoFinal.Views;
public partial class InventoryPage : ContentPage
{
    readonly ProductStore store = new();
    public InventoryPage() => InitializeComponent();
    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    async Task LoadAsync() => ProductList.ItemsSource = await store.GetAsync();
    async void ConsumeClicked(object sender, EventArgs e) { if (sender is Button button && button.CommandParameter is Guid id) { await store.ConsumeAsync(id); await LoadAsync(); } }
    async void WasteClicked(object sender, EventArgs e) { if (sender is Button button && button.CommandParameter is Guid id) { await store.WasteAsync(id); await LoadAsync(); } }
}
