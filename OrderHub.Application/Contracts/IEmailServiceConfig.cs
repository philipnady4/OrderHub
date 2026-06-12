namespace OrderHub.Application.Contracts;
/// <summary>
/// Configuration interface for EmailService
/// </summary>
public interface IEmailServiceConfig
{
    string SmtpHost { get; }
    int SmtpPort { get; }
    string SmtpUsername { get; }
    string SmtpPassword { get; }
    bool EnableSsl { get; }
    bool UseDefaultCredentials { get; }
    string FromAddress { get; }
    bool IsHtmlBody { get; }
}