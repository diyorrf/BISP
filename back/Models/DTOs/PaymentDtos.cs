namespace back.Models.DTOs;

public record PlanDto(
    string Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int DailyTokens,
    int MaxDocuments,
    string[] Features
);

public record ProcessPaymentRequest(
    string Plan,
    string PaymentToken,
    string? TransactionId
);

public record PaymentResultDto(
    bool Success,
    string Message,
    string Plan,
    int TokensRemaining,
    DateTime? PlanExpiresAt
);

public record PaymentHistoryDto(
    Guid Id,
    string Plan,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? PlanExpiresAt
);
