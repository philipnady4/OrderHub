using Microsoft.EntityFrameworkCore;
using OrderHub.Domain;
using OrderHub.Domain.Data;

namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for School entity
/// </summary>
public class SchoolRepository : Repository<School>, ISchoolRepository
{
    public SchoolRepository(OrderHubDBContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<School>> GetSchoolsByTierCodeAsync(string tierCode)
    {
        return await _dbSet
            .Where(s => s.TierCode == tierCode)
            .ToListAsync();
    }
}
