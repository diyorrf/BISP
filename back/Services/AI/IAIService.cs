using back.Models.DTOs;

namespace back.Services.AI
{
    public interface IAIService
    {
        Task<string> GetAnswerAsync(string documentContent, string question, CancellationToken ct = default);
        Task<string> GetAnswerAsync(string documentContent, string question, List<ChatMessageDto>? history, CancellationToken ct = default);
        IAsyncEnumerable<string> GetAnswerStreamAsync(string documentContent, string question, CancellationToken ct = default);
        IAsyncEnumerable<string> GetAnswerStreamAsync(string documentContent, string question, List<ChatMessageDto>? history, CancellationToken ct = default);
    }
}
