using ProyectoFinal.Services;

namespace ProyectoFinal.Views;

public partial class AlertsPage : ContentPage
{
    private readonly ProductStore store = new();

    public AlertsPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var products = await store.GetAsync();
        var alerts = products.Where(p => !p.IsConsumed && !p.IsWasted && p.DaysUntilExpiration <= 3)
            .OrderBy(p => p.ExpirationDate)
            .ToList();

        AlertList.ItemsSource = alerts;
        RecipeList.ItemsSource = RecipeSuggestionService.GetSuggestions(products);
        Summary.Text = alerts.Count == 0
            ? "No tienes productos próximos a vencer."
            : $"Tienes {alerts.Count} producto(s) que requieren atención.";
    }
}
