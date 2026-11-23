using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services;

public static class NotificationService
{
    public static async Task ShowWelcomeAsync(string email)
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
        await LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            NotificationId = 1001,
            Title = "Bienvenido a EcoScan Hogar",
            Description = $"Sesión iniciada para {email}."
        });
    }

    public static async Task ShowExpiryAlertsAsync(IEnumerable<ProductItem> products)
    {
        var items = products.Where(x => !x.IsConsumed && !x.IsWasted && x.DaysUntilExpiration is >= 0 and <= 3).ToList();
        if (items.Count == 0)
            return;

        // El permiso ya se solicita en el inicio de sesión; aquí solo se publica la alerta.
        await LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            NotificationId = 1002,
            Title = "Alimentos próximos a vencer",
            Description = $"Revisa: {string.Join(", ", items.Take(3).Select(x => x.Name))}."
        });
    }
}
