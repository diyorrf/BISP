using back.Data.Repos.Interfaces;
using back.Extensions;
using back.Models;
using back.Models.DTOs;
using back.Services.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IQuestionRepository _questionRepository;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(
            IQuestionService questionService,
            IQuestionRepository questionRepository,
            ILogger<QuestionsController> logger)
        {
            _questionService = questionService;
            _questionRepository = questionRepository;
            _logger = logger;
        }

        private long? UserId => User.GetUserId();

        [HttpGet("document/{documentId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<ChatHistoryItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChatHistory(Guid documentId, CancellationToken ct)
        {
            if (UserId is not { } userId)
                return Unauthorized();

            var questions = await _questionRepository.GetByDocumentIdAndUserIdAsync(documentId, userId, ct);
            var history = questions.Select(q => new ChatHistoryItemDto(
                q.Id,
                q.QuestionText,
                q.Answer ?? "",
                q.AskedAt,
                q.AnsweredAt
            ));

            return Ok(history);
        }

        [HttpPost]
        [ProducesResponseType(typeof(QuestionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<QuestionResponseDto>> AskQuestion(
            [FromBody] QuestionRequestDto request,
            CancellationToken ct)
        {
            if (UserId is not { } userId)
                return Unauthorized();
            if (string.IsNullOrWhiteSpace(request.QuestionText))
                return BadRequest("Question text is required");

            try
            {
                var response = await _questionService.AskQuestionAsync(request, userId, ct);
                return Ok(response);
            }
            catch (InsufficientTokensException ex)
            {
                return StatusCode(402, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing question");
                return StatusCode(500, "An error occurred while processing your question");
            }
        }
    }
}