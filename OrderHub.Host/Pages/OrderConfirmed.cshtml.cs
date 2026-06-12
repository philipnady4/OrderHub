using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Application.Services;

namespace OrderHub1.Pages
{
    public class OrderConfirmedModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public OrderConfirmedModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            
            return Page();
        }
    }
}
