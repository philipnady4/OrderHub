using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Application.Services;


namespace OrderHub1.Pages
{
    public class OrderLineViewModel
    {
        public int Id { get; set; }
        public string Sku { get; set; } = "";
        public string? Embroidery { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
    public class ConfirmOrderModel : PageModel
    {
        private readonly OrderApplicationService _orderApplicationService;

        public ConfirmOrderModel(OrderApplicationService orderApplicationService)
        {
            _orderApplicationService = orderApplicationService;
        }

        public string SchoolName { get; set; } = "";
        public List<OrderLineViewModel> Lines { get; set; } = new();
        public decimal Subtotal => Lines.Sum(l => l.LineTotal);

        // Bound from inputs named Quantities[lineId]
        [BindProperty]
        public Dictionary<int, int> Quantities { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var order = await _orderApplicationService.GetOrderForConfirmationAsync(orderId);
            if (order is null) return NotFound();

            SchoolName = order.SchoolName;
            Lines = order.Lines
                .Select(l => new OrderLineViewModel
                {
                    Id = l.Id,
                    Sku = l.Sku,
                    Embroidery = l.Embroidery,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int orderId)
        {
            var order = await _orderApplicationService.GetOrderForConfirmationAsync(orderId);
            if (order is null) return NotFound();

            SchoolName = order.SchoolName;
            Lines = order.Lines
                .Select(l => new OrderLineViewModel
                {
                    Id = l.Id,
                    Sku = l.Sku,
                    Embroidery = l.Embroidery,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                })
                .ToList();

            // Apply submitted quantities, server-side, with validation —
            // never trust a client-calculated subtotal.
            foreach (var line in Lines)
            {
                if (Quantities.TryGetValue(line.Id, out var qty))
                {
                    if (qty < 0)
                    {
                        ModelState.AddModelError(string.Empty,
                            $"Quantity for {line.Sku} cannot be negative.");
                        continue;
                    }
                    line.Quantity = qty;
                }
            }

            if (!ModelState.IsValid) return Page();

            await _orderApplicationService.UpdateLinesAsync(orderId, Lines.Cast<object>().ToList());
            await _orderApplicationService.ConfirmOrderAsync(orderId, Subtotal);

            return RedirectToPage("OrderConfirmed", new { orderId });
        }
    }
}