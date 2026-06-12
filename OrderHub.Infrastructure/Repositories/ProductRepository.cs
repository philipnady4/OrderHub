using Microsoft.EntityFrameworkCore;
using OrderHub.Domain;
using OrderHub.Domain.Data;

namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Product entity
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(OrderHubDBContext dbContext) : base(dbContext)
    {
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<IEnumerable<Product?>> GetProductsBySkusAsync(IEnumerable<string> skus)
    {
        return await _dbSet
            .Where(p => skus.Contains(p.Sku)).ToListAsync();
    }
}
