namespace OrderHub.Application.Contracts;

/// <summary>
/// Interface for payment processing operations
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Process payment for an order
    /// </summary>
    /// <param name="amount">Order amount to charge</param>
    /// <param name="email">Customer email for payment receipt</param>
    /// <returns>True if payment succeeded, false otherwise</returns>
    Task<bool> ProcessPaymentAsync(decimal amount, string email);
}
