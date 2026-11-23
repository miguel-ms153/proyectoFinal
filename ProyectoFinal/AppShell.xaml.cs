namespace ProyectoFinal;

public partial class AppShell : Shell
{
    public AppShell() => InitializeComponent();

    private async void LogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            "Cerrar sesión",
            "¿Deseas salir para ingresar con otra cuenta?",
            "Cerrar sesión",
            "Cancelar");

        if (!confirmed)
            return;

        Preferences.Default.Remove("ecoscan_user");
        Application.Current!.Windows[0].Page = new Views.LoginPage();
    }
}
