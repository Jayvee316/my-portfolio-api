using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MyPortfolio.Core.Interfaces;

namespace MyPortfolio.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _configuration["Email:SenderName"] ?? "Portfolio Contact",
                _configuration["Email:SenderEmail"] ?? "noreply@example.com"
            ));

            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            var host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_configuration["Email:SmtpPort"] ?? "587");

            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

            var senderEmail = _configuration["Email:SenderEmail"];
            var senderPassword = _configuration["Email:SenderPassword"];

            if (!string.IsNullOrEmpty(senderEmail) && !string.IsNullOrEmpty(senderPassword))
            {
                await smtp.AuthenticateAsync(senderEmail, senderPassword);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendContactNotificationAsync(string name, string email, string subject, string message)
    {
        var adminEmail = _configuration["Email:AdminEmail"] ?? _configuration["Email:SenderEmail"];

        if (string.IsNullOrEmpty(adminEmail))
        {
            _logger.LogWarning("No admin email configured, skipping notification");
            return false;
        }

        var body = $@"
            <h2>New Contact Form Submission</h2>
            <p><strong>From:</strong> {name} ({email})</p>
            <p><strong>Subject:</strong> {subject}</p>
            <hr />
            <p><strong>Message:</strong></p>
            <p>{message}</p>
            <hr />
            <p><em>Sent from Portfolio Contact Form</em></p>
        ";

        return await SendEmailAsync(adminEmail, $"Contact Form: {subject}", body);
    }
}
