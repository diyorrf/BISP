using back.Extensions;
using back.Models.DTOs;
using back.Services.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        private long? UserId => User.GetUserId();

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentDto>> UploadDocument(IFormFile file, CancellationToken ct)
        {
            if (UserId is not { } userId)
                return Unauthorized();
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                var uploadDto = new DocumentUploadDto(file);
                var result = await _documentService.UploadDocumentAsync(uploadDto, userId, ct);
                return CreatedAtAction(nameof(GetDocument), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentDto>> GetDocument(Guid id, CancellationToken ct)
        {
            if (UserId is not { } userId)
                return Unauthorized();
            var document = await _documentService.GetDocumentAsync(id, userId, ct);
            if (document == null)
                return NotFound();
            return Ok(document);
        }

        [HttpGet("{id:guid}/detail")]
        [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentDetailDto>> GetDocumentDetail(Guid id, CancellationToken ct)
        {
            if (UserId is not { } userId)
                return Unauthorized();

            var document = await _documentService.GetDocumentDetailAsync(id, userId, ct);
            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAllDocuments(CancellationToken ct)
        {
            if (UserId is not { } userId)
                return Unauthorized();
            var documents = await _documentService.GetAllDocumentsAsync(userId, ct);
            return Ok(documents);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken ct)
        {
            if (UserId is not { } userId)
                return Unauthorized();
            var deleted = await _documentService.DeleteDocumentAsync(id, userId, ct);
            return deleted ? NoContent() : NotFound();
        }
    }
}