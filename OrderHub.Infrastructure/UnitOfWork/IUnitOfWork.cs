using OrderHub.Infrastructure.Repositories;

namespace OrderHub.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work interface to coordinate all repositories and manage transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IOrderLineRepository OrderLines { get; }
    IProductRepository Products { get; }
    ISchoolRepository Schools { get; }
    IStockRepository Stocks { get; }

    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}
