using Microsoft.Extensions.Logging;
using OrderHub.Application.Contracts;
using OrderHub.Application.DTOs;
using OrderHub.Domain;
using OrderHub.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Application.Services
{
    public class OrderApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderApplicationService> _logger;

        public OrderApplicationService(
            IUnitOfWork unitOfWork,
            IPaymentService paymentService,
            IEmailService emailService,
            ILogger<OrderApplicationService> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve order data for confirmation page display
        /// </summary>
        public async Task<OrderForConfirmationDto?> GetOrderForConfirmationAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.OrderLines.FirstOrDefaultAsync(ol => ol.Id == orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order not found: {orderId}");
                    return null;
                }

                // Get all order lines (assuming we query by some common identifier)
                // For now, we'll fetch orderlines and reconstruct order data
                var orderLines = await _unitOfWork.OrderLines.GetAllAsync();

                // Get products for pricing
                var skus = orderLines.Select(ol => ol.Sku).Distinct();
                var products = await _unitOfWork.Products.GetProductsBySkusAsync(skus);

                // Get school info - this needs to be associated somehow
                // For a simple implementation, we'll use the first line to lookup context
                var product = products.FirstOrDefault();

                var confirmationDto = new OrderForConfirmationDto
                {
                    Id = orderId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Today),
                    Season = "Current",
                    Lines = orderLines.Select(ol => new OrderLineForConfirmationDto
                    {
                        Id = ol.Id,
                        Sku = ol.Sku,
                        Embroidery = ol.Embroidery,
                        Quantity = ol.Quantity,
                        UnitPrice = products.FirstOrDefault(p => p.Sku == ol.Sku)?.BasePrice ?? 0m
                    }).ToList()
                };

                return confirmationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order {orderId} for confirmation");
                return null;
            }
        }

        /// <summary>
        /// Update order line quantities before confirmation
        /// </summary>
        public async Task<bool> UpdateLinesAsync(int orderId, List<object> updatedLines)
        {
            try
            {
                _logger.LogInformation($"Updating order lines for order {orderId}");

                var lines = await _unitOfWork.OrderLines.GetAllAsync();

                foreach (var line in lines)
                {
                    var updated = updatedLines.FirstOrDefault(ul => 
                    {
                        var idProp = ul?.GetType().GetProperty("Id");
                        return idProp?.GetValue(ul) as int? == line.Id;
                    });

                    if (updated != null)
                    {
                        var qtyProp = updated.GetType().GetProperty("Quantity");
                        if (qtyProp != null && qtyProp.GetValue(updated) is int qty)
                        {
                            line.Quantity = qty;
                            _unitOfWork.OrderLines.Update(line);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Order lines updated successfully for order {orderId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order lines for order {orderId}");
                return false;
            }
        }

        /// <summary>
        /// Confirm and finalize an order, process payment
        /// </summary>
        public async Task<OrderProcessingResult> ConfirmOrderAsync(int orderId, decimal orderTotal)
        {
            try
            {
                _logger.LogInformation($"Confirming order {orderId} with total £{orderTotal}");

                // Get order details
                var orderLines = await _unitOfWork.OrderLines.GetAllAsync();
                var skus = orderLines.Select(ol => ol.Sku).Distinct();
                var products = await _unitOfWork.Products.GetProductsBySkusAsync(skus);

                // Validate stock availability before final confirmation
                var stocks = await _unitOfWork.Stocks.GetStocksBySkusAsync(skus);
                foreach (var line in orderLines)
                {
                    var stock = stocks.FirstOrDefault(s => s.Sku == line.Sku);
                    if (stock == null || stock.Qty < line.Quantity)
                    {
                        _logger.LogWarning($"Stock validation failed for {line.Sku}");
                        return OrderProcessingResult.Failure($"Out of stock: {line.Sku}", "OUT_OF_STOCK");
                    }
                }

                // Process payment (mocked - in real scenario would have email)
                var paymentResult = await _paymentService.ProcessPaymentAsync(orderTotal, "orders@brindleford.co.uk");
                if (!paymentResult)
                {
                    _logger.LogWarning($"Payment failed for order {orderId}");
                    return OrderProcessingResult.Failure("Payment processing failed", "PAYMENT_FAILED");
                }

                // Update order status
                // Note: Would need proper Order entity with OrderId foreign key in OrderLine
                // For now, we'll just save the line quantity changes that were already made

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Order {orderId} confirmed successfully");
                return OrderProcessingResult.Success(orderTotal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming order {orderId}");
                return OrderProcessingResult.Failure("An error occurred while confirming the order", "ORDER_CONFIRMATION_ERROR");
            }
        }

        /// <summary>
        /// Process an order for a school with order lines and parent email
        /// </summary>
        public async Task<OrderProcessingResult> ProcessOrderAsync(int schoolId, List<OrderLine> lines, string parentEmail)
        {
            try
            {
                // Validate input
                if (lines == null || !lines.Any())
                {
                    return OrderProcessingResult.Failure("Order must contain at least one line item", "EMPTY_ORDER");
                }

                if (string.IsNullOrWhiteSpace(parentEmail))
                {
                    return OrderProcessingResult.Failure("Parent email is required", "MISSING_EMAIL");
                }

                // Get school and verify it exists
                var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
                if (school == null)
                {
                    _logger.LogWarning($"School not found: {schoolId}");
                    return OrderProcessingResult.Failure("School not found", "SCHOOL_NOT_FOUND");
                }

                _logger.LogInformation($"Processing order for School {schoolId} (Tier: {school.TierCode})");

                // Calculate order total with tier discounts and embroidery charges
                decimal orderTotal = 0;
                var lineDetails = new List<OrderLineDetail>();
                var skus = lines.Select(x => x.Sku);
                 // get Products and Stocks with one Db call 
                var products = await _unitOfWork.Products.GetProductsBySkusAsync(skus);
                var stocks = await _unitOfWork.Stocks.GetStocksBySkusAsync(skus);

                foreach (var line in lines)
                {
                    // Get product
                    var product = products.FirstOrDefault(x => x!=null && x.Sku == line.Sku);
                    if (product == null)
                    {
                        _logger.LogWarning($"Product not found: {line.Sku}");
                        return OrderProcessingResult.Failure($"Product not found: {line.Sku}", "PRODUCT_NOT_FOUND");
                    }

                    // Check stock availability
                    var stock = stocks.FirstOrDefault(x => x != null && x.Sku == line.Sku);
                    if (stock == null || stock.Qty < line.Quantity)
                    {
                        _logger.LogWarning($"Insufficient stock for {line.Sku}: requested {line.Quantity}, available {stock?.Qty ?? 0}");
                        return OrderProcessingResult.Failure($"Out of stock: {line.Sku}", "OUT_OF_STOCK");
                    }

                    // Calculate price with tier discount
                    decimal linePrice = product.BasePrice;
                    decimal discountedPrice = ApplyTierDiscount(linePrice, school.TierCode);

                    // Add embroidery charge if applicable
                    if (!string.IsNullOrEmpty(line.Embroidery))
                    {
                        discountedPrice += CalculateEmbroideryCharge(line.Embroidery);
                    }

                    decimal lineTotal = discountedPrice * line.Quantity;
                    orderTotal += lineTotal;

                    lineDetails.Add(new OrderLineDetail
                    {
                        Sku = line.Sku,
                        Quantity = line.Quantity,
                        BasePrice = product.BasePrice,
                        DiscountedPrice = discountedPrice,
                        LineTotal = lineTotal
                    });
                }

                _logger.LogInformation($"Order total calculated: £{orderTotal}");

                // Process payment
                bool paymentSucceeded = await _paymentService.ProcessPaymentAsync(orderTotal, parentEmail);
                if (!paymentSucceeded)
                {
                    _logger.LogWarning($"Payment failed for {parentEmail}");
                    return OrderProcessingResult.Failure("Payment processing failed", "PAYMENT_FAILED");
                }

                // Send confirmation email (non-blocking failure)
                string emailBody = FormatOrderConfirmationEmail(lineDetails, orderTotal);
                var emailSent = await _emailService.SendOrderConfirmationAsync(
                    parentEmail,
                    "Order Confirmed",
                    emailBody);

                if (!emailSent)
                {
                    _logger.LogWarning($"Failed to send confirmation email to {parentEmail}, but order was processed");
                }

                _logger.LogInformation($"Order processed successfully for {parentEmail}. Total: £{orderTotal}");
                return OrderProcessingResult.Success(orderTotal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order");
                return OrderProcessingResult.Failure("An error occurred while processing the order", "ORDER_PROCESSING_ERROR");
            }
        }


        /// <summary>
        /// Apply tier-based discount to product price
        /// </summary>
        private decimal ApplyTierDiscount(decimal basePrice, string tierCode)
        {
            return tierCode switch
            {
                "GOLD" => basePrice * 0.85m,      
                "SILVER" => basePrice * 0.92m,   
                _ => basePrice                     
            };
        }

        /// <summary>
        /// Calculate embroidery charge based on text length
        /// </summary>
        private decimal CalculateEmbroideryCharge(string embroidery)
        {
            if (string.IsNullOrEmpty(embroidery))
                return 0;

            return embroidery.Length <= 3 ? 4.50m : 8.00m;
        }

        /// <summary>
        /// Format order confirmation email body
        /// </summary>
        private string FormatOrderConfirmationEmail(List<OrderLineDetail> lineDetails, decimal orderTotal)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Your order has been confirmed!");
            sb.AppendLine();
            sb.AppendLine("Order Details:");
            sb.AppendLine(new string('-', 50));

            foreach (var line in lineDetails)
            {
                sb.AppendLine($"SKU: {line.Sku}");
                sb.AppendLine($"  Quantity: {line.Quantity}");
                sb.AppendLine($"  Base Price: £{line.BasePrice:F2}");
                sb.AppendLine($"  After Discount: £{line.DiscountedPrice:F2}");
                sb.AppendLine($"  Line Total: £{line.LineTotal:F2}");
                sb.AppendLine();
            }

            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"Order Total: £{orderTotal:F2}");
            sb.AppendLine();
            sb.AppendLine("Thank you for your order!");

            return sb.ToString();
        }

       
        
    }
}
