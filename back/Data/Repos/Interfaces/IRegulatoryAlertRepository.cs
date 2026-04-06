using back.Data.Entities;

namespace back.Data.Repos.Interfaces;

public interface IRegulatoryAlertRepository
{
    Task<IEnumerable<RegulatoryAlert>> GetByUserIdAsync(long userId, bool? isRead = null, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default);
    Task<RegulatoryAlert?> GetByIdAndUserIdAsync(Guid id, long userId, CancellationToken ct = default);
    Task AddAsync(RegulatoryAlert alert, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<RegulatoryAlert> alerts, CancellationToken ct = default);
    Task UpdateAsync(RegulatoryAlert alert, CancellationToken ct = default);
    Task MarkAllReadAsync(long userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid regulatoryUpdateId, Guid legalReferenceId, CancellationToken ct = default);
    Task<bool> ExistsForDocumentAsync(Guid regulatoryUpdateId, Guid documentId, CancellationToken ct = default);
}
