using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.Models.DTOs
{
    public record DocumentUploadDto(IFormFile File);

    public record DocumentDto(
        Guid Id,
        string FileName,
        string ContentType,
        long SizeInBytes,
        DateTime UploadedAt
    );

    public record ChatMessageDto(
        string Role,
        string Content
    );

    public record QuestionRequestDto(
        Guid DocumentId,
        string QuestionText,
        List<ChatMessageDto>? History = null
    );

    public record QuestionResponseDto(
        Guid QuestionId,
        string Answer,
        int TokensUsed,
        TimeSpan ProcessingTime
    );

    public record StreamChunkDto(
        string Content,
        bool IsComplete
    );
}