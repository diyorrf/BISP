using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models.DTOs;

namespace back.Services.Document
{
    public interface IDocumentService
    {
        Task<DocumentDto> UploadDocumentAsync(DocumentUploadDto uploadDto, long userId, CancellationToken ct = default);
        Task<DocumentDto?> GetDocumentAsync(Guid id, long userId, CancellationToken ct = default);
        Task<DocumentDetailDto?> GetDocumentDetailAsync(Guid id, long userId, CancellationToken ct = default);
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(long userId, CancellationToken ct = default);
        Task<bool> DeleteDocumentAsync(Guid id, long userId, CancellationToken ct = default);
    }
}