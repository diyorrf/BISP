namespace back.Models.DTOs;

public record RegulatoryAlertDto(
    Guid Id,
    Guid DocumentId,
    string DocumentName,
    string UpdateTitle,
    string UpdateDescription,
    string LawReference,
    DateTime? EffectiveDate,
    bool IsRead,
    DateTime CreatedAt,
    string? RiskDescription = null
);

public record RegulatoryUpdateDto(
    Guid Id,
    string Title,
    string Description,
    string LawIdentifier,
    string? SourceUrl,
    DateTime? EffectiveDate,
    DateTime PublishedAt,
    bool IsProcessed
);

public record CreateRegulatoryUpdateRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string LawIdentifier { get; init; } = string.Empty;
    public string? SourceUrl { get; init; }
    public DateTime? EffectiveDate { get; init; }
    public DateTime PublishedAt { get; init; }
}

public record LegalReferenceDto(
    Guid Id,
    Guid DocumentId,
    string Title,
    string ArticleOrSection,
    string RawText,
    string Jurisdiction,
    DateTime ExtractedAt
);
