using System;
using System.Collections.Generic;
using OrderHub.Domain;

namespace OrderHub.Application.Tests.Fixtures;

/// <summary>
/// Provides mock test data for order processing tests
/// </summary>
public class MockDataFixture
{
    public static School CreateSchool(int id = 1, string tierCode = "STANDARD")
    {
        return new School
        {
            Id = id,
            TierCode = tierCode
        };
    }

    public static Product CreateProduct(string sku = "SKU001", decimal basePrice = 100.00m)
    {
        return new Product
        {
            Id = 1,
            Sku = sku,
            BasePrice = basePrice
        };
    }

    public static Stock CreateStock(string sku = "SKU001", int qty = 100)
    {
        return new Stock
        {
            Id = 1,
            Sku = sku,
            Qty = qty
        };
    }

    public static OrderLine CreateOrderLine(string sku = "SKU001", int quantity = 1, string? embroidery = null)
    {
        return new OrderLine
        {
            Id = 1,
            Sku = sku,
            Quantity = quantity,
            Embroidery = embroidery
        };
    }

    public static List<OrderLine> CreateOrderLines(params (string sku, int qty, string? embroidery)[] items)
    {
        var lines = new List<OrderLine>();
        foreach (var (sku, qty, embroidery) in items)
        {
            lines.Add(CreateOrderLine(sku, qty, embroidery));
        }
        return lines;
    }
}
