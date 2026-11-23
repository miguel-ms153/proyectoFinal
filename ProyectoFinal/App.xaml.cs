using Microsoft.Extensions.DependencyInjection;

namespace ProyectoFinal
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Page startPage = Preferences.Default.ContainsKey("ecoscan_user") ? new AppShell() : new Views.LoginPage();
            return new Window(startPage);
        }
    }
}
