using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models.DTOs;

namespace back.Services.Question
{
    public interface IQuestionService
    {
        Task<QuestionResponseDto> AskQuestionAsync(QuestionRequestDto request, long userId, CancellationToken ct = default);
        IAsyncEnumerable<StreamChunkDto> AskQuestionStreamAsync(QuestionRequestDto request, long userId, CancellationToken ct = default);
    }
}