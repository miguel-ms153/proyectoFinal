using System.Net;
using System.Net.Http.Json;
using ProyectoFinal.Services;
namespace ProyectoFinal.Views;
public partial class LoginPage : ContentPage
{
    static readonly HttpClient Client = new() { BaseAddress = new Uri("http://10.0.2.2:5188/") };
    public LoginPage() => InitializeComponent();
    async void LoginClicked(object sender, EventArgs e) => await AuthenticateAsync("api/auth/login", false);
    async void RegisterClicked(object sender, EventArgs e) => await AuthenticateAsync("api/auth/register", true);
    async Task AuthenticateAsync(string path, bool includeName)
    {
        MessageLabel.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text) || (includeName && string.IsNullOrWhiteSpace(NameEntry.Text))) { MessageLabel.Text = "Completa los campos requeridos."; return; }
        try { var response = await Client.PostAsJsonAsync(path, new { Name = NameEntry.Text ?? string.Empty, Email = EmailEntry.Text, Password = PasswordEntry.Text }); if (!response.IsSuccessStatusCode) { MessageLabel.Text = response.StatusCode == HttpStatusCode.Unauthorized ? "Correo o contraseña incorrectos." : "No se pudo completar la operación."; return; } Preferences.Default.Set("ecoscan_user", EmailEntry.Text); await NotificationService.ShowWelcomeAsync(EmailEntry.Text); Application.Current!.Windows[0].Page = new AppShell(); }
        catch { MessageLabel.Text = "No se pudo conectar con el servicio. Verifica que la API esté iniciada."; }
    }
}
