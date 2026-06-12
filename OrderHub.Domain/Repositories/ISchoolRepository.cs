using OrderHub.Domain;

namespace OrderHub.Infrastructure.Repositories;

/// <summary>
/// Repository interface for School entity with entity-specific operations
/// </summary>
public interface ISchoolRepository : IRepository<School>
{
    Task<IEnumerable<School>> GetSchoolsByTierCodeAsync(string tierCode);
}
