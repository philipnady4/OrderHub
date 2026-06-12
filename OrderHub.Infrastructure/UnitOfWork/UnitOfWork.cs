using Microsoft.EntityFrameworkCore.Storage;
using OrderHub.Domain.Data;
using OrderHub.Infrastructure.Repositories;

namespace OrderHub.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work implementation that manages all repositories and DbContext transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderHubDBContext _dbContext;
    private IDbContextTransaction? _transaction;

    private IOrderLineRepository? _orderLineRepository;
    private IProductRepository? _productRepository;
    private ISchoolRepository? _schoolRepository;
    private IStockRepository? _stockRepository;

    public UnitOfWork(OrderHubDBContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IOrderLineRepository OrderLines => _orderLineRepository ??= new OrderLineRepository(_dbContext);
    public IProductRepository Products => _productRepository ??= new ProductRepository(_dbContext);
    public ISchoolRepository Schools => _schoolRepository ??= new SchoolRepository(_dbContext);
    public IStockRepository Stocks => _stockRepository ??= new StockRepository(_dbContext);

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> BeginTransactionAsync()
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync();
        return _transaction != null;
    }

    public async Task<bool> CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            await _transaction?.CommitAsync()!;
            return true;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task<bool> RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync()!;
            return true;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _dbContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}
