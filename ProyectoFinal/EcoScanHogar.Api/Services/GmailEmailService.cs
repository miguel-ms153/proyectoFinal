using System.Net;
using System.Net.Mail;

namespace EcoScanHogar.Api.Services;

public sealed class GmailEmailService(IConfiguration configuration)
{
    public async Task SendResetCodeAsync(string recipient, string code)
    {
        var sender = configuration["EmailSettings:Sender"];
        var appPassword = configuration["EmailSettings:AppPassword"]?.Replace(" ", string.Empty);
        if (string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(appPassword))
            throw new InvalidOperationException("El correo de recuperación aún no está configurado.");

        using var message = new MailMessage(sender, recipient)
        {
            Subject = "Código de recuperación - EcoScan Hogar",
            Body = $"Tu código de recuperación es: {code}\n\nEste código vence en 15 minutos. Si no solicitaste el cambio, ignora este mensaje."
        };
        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(sender, appPassword)
        };
        await smtp.SendMailAsync(message);
    }
}
