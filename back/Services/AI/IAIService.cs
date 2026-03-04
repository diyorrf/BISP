using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.Services.AI
{
    public interface IAIService
    {
        Task<string> GetAnswerAsync(string documentContent, string question, CancellationToken ct = default);
        IAsyncEnumerable<string> GetAnswerStreamAsync(string documentContent, string question, CancellationToken ct = default);
    }
}