namespace OrderHub.Application.Services.Contracts;

/// <summary>
/// Configuration interface for PaymentService
/// </summary>
public interface IPaymentServiceConfig
{
    string PaymentApiUrl { get; }
}
