using System.Net.Http.Json;

namespace ProyectoFinal.Views;

public partial class PasswordRecoveryPage : ContentPage
{
    private static readonly HttpClient Client = new() { BaseAddress = new Uri("http://10.0.2.2:5188/") };

    public PasswordRecoveryPage() => InitializeComponent();

    private async void SendCodeClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            MessageLabel.Text = "Ingresa tu correo electrónico.";
            return;
        }

        try
        {
            var response = await Client.PostAsJsonAsync("api/auth/password-reset/request", new { Email = EmailEntry.Text.Trim() });
            MessageLabel.TextColor = response.IsSuccessStatusCode ? Color.FromArgb("#19704F") : Color.FromArgb("#A63D40");
            MessageLabel.Text = response.IsSuccessStatusCode
                ? "Si existe una cuenta con ese correo, recibirás un código en unos segundos."
                : "No se pudo enviar el código. Verifica que la API y Gmail estén configurados.";
        }
        catch
        {
            MessageLabel.TextColor = Color.FromArgb("#A63D40");
            MessageLabel.Text = "No se pudo conectar con el servicio.";
        }
    }

    private async void ResetPasswordClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(CodeEntry.Text) || string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
        {
            MessageLabel.Text = "Completa correo, código y nueva contraseña.";
            return;
        }

        try
        {
            var response = await Client.PostAsJsonAsync("api/auth/password-reset/confirm", new { Email = EmailEntry.Text.Trim(), Code = CodeEntry.Text.Trim(), NewPassword = NewPasswordEntry.Text });
            MessageLabel.TextColor = response.IsSuccessStatusCode ? Color.FromArgb("#19704F") : Color.FromArgb("#A63D40");
            MessageLabel.Text = response.IsSuccessStatusCode ? "Contraseña actualizada. Ya puedes iniciar sesión." : "El código es incorrecto, venció o la contraseña es muy corta.";
        }
        catch
        {
            MessageLabel.TextColor = Color.FromArgb("#A63D40");
            MessageLabel.Text = "No se pudo conectar con el servicio.";
        }
    }

    private void BackClicked(object sender, EventArgs e) => Application.Current!.Windows[0].Page = new LoginPage();
}
