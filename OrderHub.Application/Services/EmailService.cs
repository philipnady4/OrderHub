using Microsoft.Extensions.Logging;
using OrderHub.Application.Contracts;
using System.Net.Mail;

namespace OrderHub.Application.Services;

/// <summary>
/// Email service implementation for sending order confirmations
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailServiceConfig _config;

    public EmailService(ILogger<EmailService> logger, IEmailServiceConfig config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<bool> SendOrderConfirmationAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Cannot send email: recipient email is empty");
            return false;
        }

        try
        {
            using var smtpClient = new SmtpClient(_config.SmtpHost, _config.SmtpPort)
            {
                EnableSsl = _config.EnableSsl,
                UseDefaultCredentials = _config.UseDefaultCredentials
            };

            if (!_config.UseDefaultCredentials && !string.IsNullOrEmpty(_config.SmtpUsername))
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);
            }

            using var mailMessage = new MailMessage(
                from: _config.FromAddress,
                to: toEmail,
                subject: subject,
                body: body)
            {
                IsBodyHtml = _config.IsHtmlBody
            };

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Order confirmation email sent to {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending order confirmation email to {toEmail}");
            // Don't fail the order if email fails
            return false;
        }
    }
}



