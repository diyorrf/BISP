using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using back.Data.Repos.Interfaces;
using back.Models.DTOs;
using back.Services.Parser;
using back.Services.Regulatory;
using DocumentEntity = back.Data.Entities.Document;

namespace back.Services.Document
{
    public class DocumentService: IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentParserService _parserService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DocumentService> _logger;
        private readonly string _storagePath;

        public DocumentService(
            IDocumentRepository documentRepository,
            IDocumentParserService parserService,
            IServiceScopeFactory scopeFactory,
            ILogger<DocumentService> logger,
            IWebHostEnvironment env)
        {
            _documentRepository = documentRepository;
            _parserService = parserService;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _storagePath = Path.Combine(env.ContentRootPath, "Storage");
            Directory.CreateDirectory(_storagePath);
        }

        public async Task<DocumentDto> UploadDocumentAsync(DocumentUploadDto uploadDto, long userId, CancellationToken ct = default)
        {
            var file = uploadDto.File;
            
            if (file.Length == 0)
                throw new ArgumentException("File is empty");

            if (!_parserService.IsSupported(file.ContentType))
                throw new NotSupportedException($"File type '{file.ContentType}' is not supported");

            var content = await _parserService.ExtractTextAsync(file, ct);

            var docId = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{docId}{extension}";
            var filePath = Path.Combine(_storagePath, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            var document = new DocumentEntity
            {
                Id = docId,
                UserId = userId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = content,
                StoredFileName = storedFileName,
                SizeInBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document, ct);
            
            _logger.LogInformation("Document {DocumentId} uploaded and stored as {StoredFile}: {FileName}", 
                document.Id, storedFileName, document.FileName);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var extractionService = scope.ServiceProvider.GetRequiredService<ILegalReferenceExtractionService>();
                    await extractionService.ExtractReferencesAsync(document.Id, content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background legal reference extraction failed for document {DocumentId}", document.Id);
                }
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var topicService = scope.ServiceProvider.GetRequiredService<ITopicExtractionService>();
                    await topicService.ExtractTopicsAsync(document.Id, content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background topic extraction failed for document {DocumentId}", document.Id);
                }
            });

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
                document.Content,
                document.StoredFileName
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
            var document = await _documentRepository.GetByIdAndUserIdAsync(id, userId, ct);
            if (document == null) return false;

            var deleted = await _documentRepository.DeleteByIdAndUserIdAsync(id, userId, ct);
            if (deleted && !string.IsNullOrEmpty(document.StoredFileName))
            {
                var filePath = Path.Combine(_storagePath, document.StoredFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Document {DocumentId} file deleted from storage: {StoredFile}", id, document.StoredFileName);
                }
            }
            return deleted;
        }

        public string? GetStoragePath(string? storedFileName)
        {
            if (string.IsNullOrEmpty(storedFileName)) return null;
            var path = Path.Combine(_storagePath, storedFileName);
            return File.Exists(path) ? path : null;
        }
    }
}