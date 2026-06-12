using Microsoft.EntityFrameworkCore;
using OrderHub.Domain;
using OrderHub.Domain.Data;

namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Stock entity
/// </summary>
public class StockRepository : Repository<Stock>, IStockRepository
{
    public StockRepository(OrderHubDBContext dbContext) : base(dbContext)
    {
    }

    public async Task<Stock?> GetStockBySkuAsync(string sku)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Sku == sku);

    }

    public async Task<IEnumerable<Stock?>> GetStocksBySkusAsync(IEnumerable<string> skus)
    {
        return await _dbSet
              .Where(p => skus.Contains(p.Sku)).ToListAsync();
    }

    public async Task<IEnumerable<Stock>> GetLowStockAsync(int threshold)
    {
        return await _dbSet
            .Where(s => s.Qty <= threshold)
            .ToListAsync();
    }
}
