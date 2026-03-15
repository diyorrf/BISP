using back.Data.Entities;
using back.Data.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repos;

public class RegulatoryUpdateRepository : IRegulatoryUpdateRepository
{
    private readonly AppDbContext _context;

    public RegulatoryUpdateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegulatoryUpdate>> GetUnprocessedAsync(CancellationToken ct = default)
    {
        return await _context.RegulatoryUpdates
            .Where(u => !u.IsProcessed)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RegulatoryUpdate>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.RegulatoryUpdates
            .OrderByDescending(u => u.PublishedAt)
            .ToListAsync(ct);
    }

    public async Task<RegulatoryUpdate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RegulatoryUpdates.FindAsync(new object[] { id }, ct);
    }

    public async Task AddAsync(RegulatoryUpdate update, CancellationToken ct = default)
    {
        await _context.RegulatoryUpdates.AddAsync(update, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RegulatoryUpdate update, CancellationToken ct = default)
    {
        _context.RegulatoryUpdates.Update(update);
        await _context.SaveChangesAsync(ct);
    }
}
