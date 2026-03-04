using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.Services.Parser
{
    public interface IDocumentParserService
    {
        Task<string> ExtractTextAsync(IFormFile file, CancellationToken ct = default);
        bool IsSupported(string contentType);
    }
}