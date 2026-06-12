using OrderHub.Application.Services.Contracts;
namespace OrderHub.Application.DTOs;
/// <summary>
/// Payment service configuration
/// </summary>
public class PaymentServiceConfig : IPaymentServiceConfig
{
    public string PaymentApiUrl { get; set; }
}