using Microsoft.AspNetCore.Identity.UI.Services;

public class ConsoleEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"To: {email}\nSubject: {subject}\nMessage: {htmlMessage}");
        return Task.CompletedTask;
    }
}
