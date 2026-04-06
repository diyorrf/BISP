using back.Data.Entities;
using back.Data.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repos;

public class RegulatoryAlertRepository : IRegulatoryAlertRepository
{
    private readonly AppDbContext _context;

    public RegulatoryAlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegulatoryAlert>> GetByUserIdAsync(long userId, bool? isRead = null, CancellationToken ct = default)
    {
        var query = _context.RegulatoryAlerts
            .Include(a => a.Document)
            .Include(a => a.RegulatoryUpdate)
            .Include(a => a.LegalReference)
            .Where(a => a.UserId == userId && !a.IsDismissed);

        if (isRead.HasValue)
            query = query.Where(a => a.IsRead == isRead.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default)
    {
        return await _context.RegulatoryAlerts
            .CountAsync(a => a.UserId == userId && !a.IsRead && !a.IsDismissed, ct);
    }

    public async Task<RegulatoryAlert?> GetByIdAndUserIdAsync(Guid id, long userId, CancellationToken ct = default)
    {
        return await _context.RegulatoryAlerts
            .Include(a => a.Document)
            .Include(a => a.RegulatoryUpdate)
            .Include(a => a.LegalReference)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
    }

    public async Task AddAsync(RegulatoryAlert alert, CancellationToken ct = default)
    {
        await _context.RegulatoryAlerts.AddAsync(alert, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<RegulatoryAlert> alerts, CancellationToken ct = default)
    {
        await _context.RegulatoryAlerts.AddRangeAsync(alerts, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RegulatoryAlert alert, CancellationToken ct = default)
    {
        _context.RegulatoryAlerts.Update(alert);
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(long userId, CancellationToken ct = default)
    {
        await _context.RegulatoryAlerts
            .Where(a => a.UserId == userId && !a.IsRead && !a.IsDismissed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.IsRead, true)
                .SetProperty(a => a.ReadAt, DateTime.UtcNow), ct);
    }

    public async Task<bool> ExistsAsync(Guid regulatoryUpdateId, Guid legalReferenceId, CancellationToken ct = default)
    {
        return await _context.RegulatoryAlerts
            .AnyAsync(a => a.RegulatoryUpdateId == regulatoryUpdateId && a.LegalReferenceId == legalReferenceId, ct);
    }

    public async Task<bool> ExistsForDocumentAsync(Guid regulatoryUpdateId, Guid documentId, CancellationToken ct = default)
    {
        return await _context.RegulatoryAlerts
            .AnyAsync(a => a.RegulatoryUpdateId == regulatoryUpdateId && a.DocumentId == documentId, ct);
    }
}
