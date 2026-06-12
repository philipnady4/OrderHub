using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OrderHub.Application.DTOs;
using OrderHub.Application.Services;
using OrderHub.Application.Tests.Fixtures;
using OrderHub.Domain;
using OrderHub.Infrastructure.Repositories;
using OrderHub.Infrastructure.UnitOfWork;
using Microsoft.Extensions.Logging;
using OrderHub.Application.Contracts;

namespace OrderHub.Application.Tests.Services;

/// <summary>
/// Unit tests for OrderApplicationService focusing on tier-based pricing business rule
/// </summary>
public class OrderApplicationServiceTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<OrderApplicationService>> _mockLogger;

    public OrderApplicationServiceTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<OrderApplicationService>>();

        // Setup default successful payment and email responses
        _mockPaymentService
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockEmailService
            .Setup(x => x.SendOrderConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    /// <summary>
    /// Verifies that GOLD tier customers receive 15% discount on product pricing
    /// </summary>
    [Fact]
    public async Task ProcessOrderAsync_WithGoldTierSchool_AppliesCorrectDiscount()
    {
        // Arrange
        var schoolId = 1;
        var goldSchool = MockDataFixture.CreateSchool(schoolId, "GOLD");
        var product = MockDataFixture.CreateProduct("SKU001", 100.00m);
        var stock = MockDataFixture.CreateStock("SKU001", 50);
        var orderLine = MockDataFixture.CreateOrderLine("SKU001", 2); // 2 units

        var mockUnitOfWork = MockUnitOfWorkFactory.CreateWithSchoolsProductsAndStocks(
            goldSchool,
            new[] { product },
            stock);

        var service = new OrderApplicationService(
            mockUnitOfWork.Object,
            _mockPaymentService.Object,
            _mockEmailService.Object,
            _mockLogger.Object);

        // Act
        var result = await service.ProcessOrderAsync(schoolId, new List<OrderLine> { orderLine }, "parent@example.com");

        // Assert
        Assert.True(result.IsSuccess);

        // Gold tier: £100 * 0.85 (15% discount) = £85 per unit
        // 2 units = £170 total
        var expectedTotal = 85.00m * 2;
        Assert.Equal(expectedTotal, result.OrderTotal);
    }

    
}
