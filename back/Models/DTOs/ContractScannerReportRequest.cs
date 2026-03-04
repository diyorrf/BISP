namespace back.Models.DTOs;

public record ContractScannerReportRequest(
    string FileName,
    string RiskLevel,
    string Summary,
    IReadOnlyList<ReportIssueDto> Issues,
    IReadOnlyList<string> Recommendations
);

public record ReportIssueDto(
    string Clause,
    string Risk,
    string Description,
    string Reference
);
