using Microsoft.Extensions.Logging;
using OrderHub.Application.Contracts;
using OrderHub.Application.Services.Contracts;

namespace OrderHub.Application.Services;

/// <summary>
/// Payment service implementation for processing payments
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;
    private readonly string _paymentApiUrl;

    public PaymentService(HttpClient httpClient, ILogger<PaymentService> logger, IPaymentServiceConfig config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _paymentApiUrl = config.PaymentApiUrl;
    }

    public async Task<bool> ProcessPaymentAsync(decimal amount, string email)
    {
        try
        {
            var body = new Dictionary<string, string>
            {
                { "amount", amount.ToString("F2") },
                { "email", email }
            };

            using var content = new FormUrlEncodedContent(body);
            var response = await _httpClient.PostAsync(_paymentApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Payment failed for {email}, amount: £{amount}. Status: {response.StatusCode}");
                return false;
            }

            _logger.LogInformation($"Payment processed successfully for {email}, amount: £{amount}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing payment for {email}, amount: £{amount}");
            return false;
        }
    }
}


