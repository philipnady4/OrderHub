namespace OrderHub.Application.Contracts;

/// <summary>
/// Interface for email sending operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send order confirmation email
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendOrderConfirmationAsync(string toEmail, string subject, string body);
}
