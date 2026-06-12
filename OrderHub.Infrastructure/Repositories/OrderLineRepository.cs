using Microsoft.EntityFrameworkCore;
using OrderHub.Domain;
using OrderHub.Domain.Data;

namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for OrderLine entity
/// </summary>
public class OrderLineRepository : Repository<OrderLine>, IOrderLineRepository
{
    public OrderLineRepository(OrderHubDBContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<OrderLine>> GetOrderLinesBySkuAsync(string sku)
    {
        return await _dbSet
            .Where(ol => ol.Sku == sku)
            .ToListAsync();
    }
}
