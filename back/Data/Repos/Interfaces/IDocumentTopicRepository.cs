using back.Data.Entities;

namespace back.Data.Repos.Interfaces;

public interface IDocumentTopicRepository
{
    Task<IEnumerable<DocumentTopic>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
    Task<IEnumerable<DocumentTopic>> GetAllWithDocumentAsync(CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<DocumentTopic> topics, CancellationToken ct = default);
    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
}
