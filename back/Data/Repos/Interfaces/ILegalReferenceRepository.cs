using back.Data.Entities;

namespace back.Data.Repos.Interfaces;

public interface ILegalReferenceRepository
{
    Task<IEnumerable<LegalReference>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
    Task<IEnumerable<LegalReference>> GetAllAsync(CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<LegalReference> references, CancellationToken ct = default);
}
