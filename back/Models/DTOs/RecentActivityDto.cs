namespace back.Models.DTOs;

public record RecentActivityItemDto(
    string Id,
    string? DocumentId,
    string Type,
    string Title,
    string Description,
    DateTime At
);
