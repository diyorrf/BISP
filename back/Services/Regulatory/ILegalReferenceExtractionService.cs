using back.Data.Entities;

namespace back.Services.Regulatory;

public interface ILegalReferenceExtractionService
{
    Task<IReadOnlyList<LegalReference>> ExtractReferencesAsync(Guid documentId, string documentContent, CancellationToken ct = default);
}
