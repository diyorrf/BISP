namespace back.Models.DTOs;

public record AdminUserDto(
    long Id,
    string Email,
    string? FullName,
    bool IsActive,
    bool EmailConfirmed,
    string Plan,
    int TokensRemaining,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    int DocumentsCount,
    int QuestionsCount,
    List<string> Roles
);

public record AdminUserDetailDto(
    long Id,
    string Email,
    string? FullName,
    bool IsActive,
    bool EmailConfirmed,
    string Plan,
    int TokensRemaining,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    DateTime? UpdatedAt,
    DateTime? LastTokenResetAt,
    int DocumentsCount,
    int QuestionsCount,
    List<string> Roles,
    List<AdminDocumentDto> Documents,
    List<AdminPaymentDto> Payments
);

public record AdminDocumentDto(
    Guid Id,
    long? UserId,
    string? UserEmail,
    string FileName,
    string ContentType,
    long SizeInBytes,
    DateTime UploadedAt,
    int QuestionsCount,
    int LegalReferencesCount
);

public record AdminDocumentReferencesDto(
    Guid DocumentId,
    string FileName,
    List<AdminLegalReferenceDto> References
);

public record AdminLegalReferenceDto(
    Guid Id,
    string Title,
    string? ArticleOrSection,
    string? Jurisdiction,
    string? RawText,
    DateTime ExtractedAt
);

public record AdminRegulatoryUpdateDto(
    Guid Id,
    string Title,
    string? Description,
    string LawIdentifier,
    string? SourceUrl,
    DateTime? EffectiveDate,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    bool IsProcessed,
    int AlertsCount
);

public record CreateRegulatoryUpdateDto(
    string Title,
    string? Description,
    string LawIdentifier,
    string? SourceUrl,
    DateTime? EffectiveDate,
    DateTime? PublishedAt
);

public record ProcessResultDto(
    Guid UpdateId,
    int AlertsCreated,
    string Message
);

public record AdminPaymentDto(
    Guid Id,
    long UserId,
    string? UserEmail,
    string Plan,
    decimal Amount,
    string Currency,
    string Status,
    string PaymentMethod,
    string? TransactionId,
    DateTime CreatedAt,
    DateTime? PlanExpiresAt
);

public record AdminStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalDocuments,
    int TotalQuestions,
    int TotalAlerts,
    int UnreadAlerts,
    int TotalPayments,
    decimal TotalRevenue,
    Dictionary<string, int> PlanBreakdown,
    int RegulatoryUpdatesTotal,
    int RegulatoryUpdatesPending
);

public record UpdateUserPlanDto(string Plan);
public record UpdateUserTokensDto(int Tokens);
