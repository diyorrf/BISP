namespace back.Models.DTOs;

public record DashboardStatsDto(
    int DocumentsScanned,
    int AiConsultations,
    int HighRiskAlerts,
    int ComplianceChecks
);
