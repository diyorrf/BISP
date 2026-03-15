namespace back.Models.DTOs;

public record AccountDto(
    string Email,
    string? FullName,
    int TokensRemaining,
    string Plan,
    bool EmailConfirmed,
    DateTime? PlanExpiresAt = null
);
