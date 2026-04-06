using back.Data.Entities;
using back.Data.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repos;

public class DocumentTopicRepository : IDocumentTopicRepository
{
    private readonly AppDbContext _context;

    public DocumentTopicRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DocumentTopic>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _context.DocumentTopics
            .Where(t => t.DocumentId == documentId)
            .OrderBy(t => t.Topic)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<DocumentTopic>> GetAllWithDocumentAsync(CancellationToken ct = default)
    {
        return await _context.DocumentTopics
            .Include(t => t.Document)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<DocumentTopic> topics, CancellationToken ct = default)
    {
        await _context.DocumentTopics.AddRangeAsync(topics, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        await _context.DocumentTopics
            .Where(t => t.DocumentId == documentId)
            .ExecuteDeleteAsync(ct);
    }
}
