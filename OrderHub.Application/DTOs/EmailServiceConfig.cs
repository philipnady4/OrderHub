using OrderHub.Application.Contracts;

namespace OrderHub.Application.DTOs;

/// <summary>
/// Email service configuration
/// </summary>
public class EmailServiceConfig : IEmailServiceConfig
{
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    public bool EnableSsl { get; set; }
    public bool UseDefaultCredentials { get; set; }
    public string FromAddress { get; set; }
    public bool IsHtmlBody { get; set; }
}
