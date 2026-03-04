using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using back.Data.Repos.Interfaces;
using back.Models.DTOs;
using back.Services.Parser;
using DocumentEntity = back.Data.Entities.Document;

namespace back.Services.Document
{
    public class DocumentService: IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentParserService _parserService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IDocumentRepository documentRepository,
            IDocumentParserService parserService,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _parserService = parserService;
            _logger = logger;
        }

        public async Task<DocumentDto> UploadDocumentAsync(DocumentUploadDto uploadDto, long userId, CancellationToken ct = default)
        {
            var file = uploadDto.File;
            
            if (file.Length == 0)
                throw new ArgumentException("File is empty");

            if (!_parserService.IsSupported(file.ContentType))
                throw new NotSupportedException($"File type '{file.ContentType}' is not supported");

            var content = await _parserService.ExtractTextAsync(file, ct);

            var document = new DocumentEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = content,
                SizeInBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document, ct);
            
            _logger.LogInformation("Document {DocumentId} uploaded: {FileName}", document.Id, document.FileName);

            return new DocumentDto(
                document.Id,
                document.FileName,
                document.ContentType,
                document.SizeInBytes,
                document.UploadedAt
            );
        }

        public async Task<DocumentDto?> GetDocumentAsync(Guid id, long userId, CancellationToken ct = default)
        {
            var document = await _documentRepository.GetByIdAndUserIdAsync(id, userId, ct);
            
            if (document == null)
                return null;

            return new DocumentDto(
                document.Id,
                document.FileName,
                document.ContentType,
                document.SizeInBytes,
                document.UploadedAt
            );
        }

        public async Task<DocumentDetailDto?> GetDocumentDetailAsync(Guid id, long userId, CancellationToken ct = default)
        {
            var document = await _documentRepository.GetByIdAndUserIdAsync(id, userId, ct);
            if (document == null)
                return null;

            // Update last accessed timestamp for auditing/UX
            document.LastAccessedAt = DateTime.UtcNow;
            await _documentRepository.UpdateAsync(document, ct);

            return new DocumentDetailDto(
                document.Id,
                document.FileName,
                document.ContentType,
                document.SizeInBytes,
                document.UploadedAt,
                document.LastAccessedAt,
                document.Content
            );
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(long userId, CancellationToken ct = default)
        {
            var documents = await _documentRepository.GetAllByUserIdAsync(userId, ct);
            
            return documents.Select(d => new DocumentDto(
                d.Id,
                d.FileName,
                d.ContentType,
                d.SizeInBytes,
                d.UploadedAt
            ));
        }

        public async Task<bool> DeleteDocumentAsync(Guid id, long userId, CancellationToken ct = default)
        {
            var deleted = await _documentRepository.DeleteByIdAndUserIdAsync(id, userId, ct);
            if (deleted)
                _logger.LogInformation("Document {DocumentId} deleted", id);
            return deleted;
        }
    }
}