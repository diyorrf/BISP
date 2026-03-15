namespace back.Models.DTOs;

public record DocumentDetailDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeInBytes,
    DateTime UploadedAt,
    DateTime? LastAccessedAt,
    string Content,
    string? StoredFileName = null
);

