using back.Data.Entities;

namespace back.Services.Regulatory;

public interface ITopicExtractionService
{
    Task<IReadOnlyList<DocumentTopic>> ExtractTopicsAsync(Guid documentId, string documentContent, CancellationToken ct = default);
}
