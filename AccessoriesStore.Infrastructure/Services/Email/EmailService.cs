using System.Net;
using System.Net.Mail;
using AccessoriesStore.Application.Abstractions.Email;
using AccessoriesStore.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace AccessoriesStore.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true)
    {
        using var message = new MailMessage();

        message.From = new MailAddress(
            _settings.FromEmail,
            _settings.FromName);

        message.To.Add(to);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = isHtml;

        using var smtpClient = new SmtpClient(
            _settings.Host,
            _settings.Port);

        smtpClient.EnableSsl = true;

        smtpClient.Credentials = new NetworkCredential(
            _settings.Username,
            _settings.Password);

        await smtpClient.SendMailAsync(message);
    }
}