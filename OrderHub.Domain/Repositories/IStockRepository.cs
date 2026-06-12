using OrderHub.Domain;

namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository interface for Stock entity with entity-specific operations
/// </summary>
public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetStockBySkuAsync(string sku);
    Task<IEnumerable<Stock?>> GetStocksBySkusAsync(IEnumerable<string> skus);
    Task<IEnumerable<Stock>> GetLowStockAsync(int threshold);
}
