using back.Extensions;
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
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(IQuestionService questionService, ILogger<QuestionsController> logger)
        {
            _questionService = questionService;
            _logger = logger;
        }

        private long? UserId => User.GetUserId();

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