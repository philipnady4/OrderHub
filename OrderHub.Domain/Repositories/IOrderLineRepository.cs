using OrderHub.Domain;
namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository interface for OrderLine entity with entity-specific operations
/// </summary>
public interface IOrderLineRepository : IRepository<OrderLine>
{
    Task<IEnumerable<OrderLine>> GetOrderLinesBySkuAsync(string sku);
}
