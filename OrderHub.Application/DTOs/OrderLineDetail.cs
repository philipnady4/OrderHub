namespace OrderHub.Application.DTOs;

public class OrderLineDetail
{
    public string Sku { get; set; }
    public int Quantity { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal LineTotal { get; set; }
}