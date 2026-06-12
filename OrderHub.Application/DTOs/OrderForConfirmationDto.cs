using System;
using System.Collections.Generic;

namespace OrderHub.Application.DTOs;

/// <summary>
/// Line item for order confirmation
/// </summary>
public class OrderLineForConfirmationDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string? Embroidery { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Complete order data for confirmation page
/// </summary>
public class OrderForConfirmationDto
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = "";
    public string Season { get; set; } = "";
    public DateOnly OrderDate { get; set; }
    public List<OrderLineForConfirmationDto> Lines { get; set; } = new();
}
