namespace MyPortfolio.Core.Interfaces;

/// <summary>
/// Interface for email operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    Task<bool> SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Send contact form notification
    /// </summary>
    Task<bool> SendContactNotificationAsync(string name, string email, string subject, string message);
}
