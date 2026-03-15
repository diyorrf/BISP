using back.Data.Entities;

namespace back.Data.Repos.Interfaces;

public interface IRegulatoryUpdateRepository
{
    Task<IEnumerable<RegulatoryUpdate>> GetUnprocessedAsync(CancellationToken ct = default);
    Task<IEnumerable<RegulatoryUpdate>> GetAllAsync(CancellationToken ct = default);
    Task<RegulatoryUpdate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(RegulatoryUpdate update, CancellationToken ct = default);
    Task UpdateAsync(RegulatoryUpdate update, CancellationToken ct = default);
}
