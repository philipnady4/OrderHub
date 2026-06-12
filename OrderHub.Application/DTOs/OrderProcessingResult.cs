namespace OrderHub.Application.DTOs;

/// <summary>
/// Result object for order processing operations
/// </summary>
public class OrderProcessingResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public decimal OrderTotal { get; set; }
    public string ErrorCode { get; set; }

    public static OrderProcessingResult Success(decimal orderTotal)
    {
        return new OrderProcessingResult
        {
            IsSuccess = true,
            Message = "Order processed successfully",
            OrderTotal = orderTotal
        };
    }

    public static OrderProcessingResult Failure(string message, string errorCode)
    {
        return new OrderProcessingResult
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}
