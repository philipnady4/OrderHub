using OrderHub.Domain;

namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository interface for Product entity with entity-specific operations
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetProductBySkuAsync(string sku);
    Task<IEnumerable< Product?>> GetProductsBySkusAsync(IEnumerable<string> skus);
}
