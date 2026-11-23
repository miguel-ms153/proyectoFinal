using ProyectoFinal.Services;
namespace ProyectoFinal.Views;
public partial class StatisticsPage : ContentPage
{
    readonly ProductStore store = new();
    public StatisticsPage() => InitializeComponent();
    protected override async void OnAppearing()
    {
        base.OnAppearing(); var products = await store.GetAsync();
        Consumed.Text = products.Where(x => x.IsConsumed).Sum(x => x.Quantity).ToString();
        Wasted.Text = products.Where(x => x.IsWasted).Sum(x => x.Quantity).ToString();
        Active.Text = products.Where(x => !x.IsConsumed && !x.IsWasted).Sum(x => x.Quantity).ToString();
    }
    async void LocationClicked(object sender, EventArgs e) { try { var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium)); LocationStatus.Text = location is null ? "No se obtuvo ubicación." : $"Ubicación registrada: {location.Latitude:F3}, {location.Longitude:F3}"; } catch { LocationStatus.Text = "Debes permitir el acceso a la ubicación."; } }
}
