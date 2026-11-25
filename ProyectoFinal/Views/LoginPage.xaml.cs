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
    void RecoveryClicked(object sender, EventArgs e) => Application.Current!.Windows[0].Page = new PasswordRecoveryPage();

    async Task AuthenticateAsync(string path, bool includeName)
    {
        MessageLabel.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text) || (includeName && string.IsNullOrWhiteSpace(NameEntry.Text)))
        {
            MessageLabel.Text = "Completa los campos requeridos.";
            return;
        }

        try
        {
            var response = await Client.PostAsJsonAsync(path, new { Name = NameEntry.Text ?? string.Empty, Email = EmailEntry.Text.Trim(), Password = PasswordEntry.Text });
            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Correo o contraseña incorrectos.",
                    HttpStatusCode.Conflict => "Este correo ya está registrado. Usa “Iniciar sesión”.",
                    HttpStatusCode.BadRequest => "Revisa los datos. La contraseña debe tener al menos 4 caracteres.",
                    _ => "No se pudo completar la operación."
                };
                return;
            }

            Preferences.Default.Set("ecoscan_user", EmailEntry.Text.Trim());
            await NotificationService.ShowWelcomeAsync(EmailEntry.Text.Trim());
            Application.Current!.Windows[0].Page = new AppShell();
        }
        catch { MessageLabel.Text = "No se pudo conectar con el servicio. Verifica que la API esté iniciada."; }
    }
}
