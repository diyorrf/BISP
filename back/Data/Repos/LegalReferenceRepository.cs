using back.Data.Entities;
using back.Data.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repos;

public class LegalReferenceRepository : ILegalReferenceRepository
{
    private readonly AppDbContext _context;

    public LegalReferenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LegalReference>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _context.LegalReferences
            .Where(r => r.DocumentId == documentId)
            .OrderBy(r => r.Title)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<LegalReference>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.LegalReferences
            .Include(r => r.Document)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<LegalReference> references, CancellationToken ct = default)
    {
        await _context.LegalReferences.AddRangeAsync(references, ct);
        await _context.SaveChangesAsync(ct);
    }
}
