using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using OrderHub.Domain;
using OrderHub.Infrastructure.Repositories;
using OrderHub.Infrastructure.UnitOfWork;

namespace OrderHub.Application.Tests.Fixtures;

/// <summary>
/// Helper to create fully mocked IUnitOfWork for testing
/// </summary>
public class MockUnitOfWorkFactory
{
    public static Mock<IUnitOfWork> Create()
    {
        var mock = new Mock<IUnitOfWork>();
        mock.Setup(x => x.Schools).Returns(new Mock<ISchoolRepository>().Object);
        mock.Setup(x => x.Products).Returns(new Mock<IProductRepository>().Object);
        mock.Setup(x => x.Stocks).Returns(new Mock<IStockRepository>().Object);
        mock.Setup(x => x.OrderLines).Returns(new Mock<IOrderLineRepository>().Object);
        return mock;
    }

    public static Mock<IUnitOfWork> CreateWithSchools(params School[] schools)
    {
        var mock = Create();
        var schoolRepo = new Mock<ISchoolRepository>();

        foreach (var school in schools)
        {
            schoolRepo
                .Setup(x => x.GetByIdAsync(school.Id))
                .ReturnsAsync(school);
        }

        mock.Setup(x => x.Schools).Returns(schoolRepo.Object);
        return mock;
    }

    public static Mock<IUnitOfWork> CreateWithSchoolsAndProducts(
        School school, 
        params Product[] products)
    {
        var mock = CreateWithSchools(school);
        var productRepo = new Mock<IProductRepository>();

        foreach (var product in products)
        {
            productRepo
                .Setup(x => x.GetProductBySkuAsync(product.Sku))
                .ReturnsAsync(product);
        }

        productRepo
            .Setup(x => x.GetProductsBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync((IEnumerable<string> skus) => 
                products.Where(p => skus.Contains(p.Sku)).ToList());

        mock.Setup(x => x.Products).Returns(productRepo.Object);
        return mock;
    }

    public static Mock<IUnitOfWork> CreateWithSchoolsProductsAndStocks(
        School school, 
        Product[] products,
        params Stock[] stocks)
    {
        var mock = CreateWithSchoolsAndProducts(school, products);
        var stockRepo = new Mock<IStockRepository>();

        foreach (var stock in stocks)
        {
            stockRepo
                .Setup(x => x.GetStockBySkuAsync(stock.Sku))
                .ReturnsAsync(stock);
        }

        stockRepo
            .Setup(x => x.GetStocksBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync((IEnumerable<string> skus) => 
                stocks.Where(s => skus.Contains(s.Sku)).ToList());

        mock.Setup(x => x.Stocks).Returns(stockRepo.Object);
        return mock;
    }
}
