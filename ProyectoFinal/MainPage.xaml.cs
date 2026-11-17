using ProyectoFinal.Services;
namespace ProyectoFinal;
public partial class MainPage : ContentPage
{
 private readonly ProductStore store = new();
 public MainPage() => InitializeComponent();
 protected override async void OnAppearing() { base.OnAppearing(); var products = await store.GetAsync(); Products.Text = products.Count.ToString(); Expiring.Text = products.Count(p => p.DaysUntilExpiration is >= 0 and <= 3 && !p.IsConsumed).ToString(); }
 private async void AddClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//add");
 private async void AlertsClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//alerts");
}
