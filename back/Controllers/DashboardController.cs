using back.Data.Repos.Interfaces;
using back.Extensions;
using back.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IQuestionRepository _questionRepository;

    public DashboardController(
        IDocumentRepository documentRepository,
        IQuestionRepository questionRepository)
    {
        _documentRepository = documentRepository;
        _questionRepository = questionRepository;
    }

    private long? UserId => User.GetUserId();

    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(CancellationToken ct)
    {
        if (UserId is not { } userId)
            return Unauthorized();

        var documentsCount = await _documentRepository.GetCountByUserIdAsync(userId, ct);
        var questionsCount = await _questionRepository.GetCountByUserIdAsync(userId, ct);

        var stats = new DashboardStatsDto(
            DocumentsScanned: documentsCount,
            AiConsultations: questionsCount,
            HighRiskAlerts: 0,
            ComplianceChecks: 0
        );

        return Ok(stats);
    }

    [HttpGet("recent-activity")]
    [ProducesResponseType(typeof(IEnumerable<RecentActivityItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RecentActivityItemDto>>> GetRecentActivity(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        if (UserId is not { } userId)
            return Unauthorized();

        var recentDocs = await _documentRepository.GetRecentByUserIdAsync(userId, count, ct);
        var recentQuestions = await _questionRepository.GetRecentByUserIdAsync(userId, count, ct);

        var activities = new List<RecentActivityItemDto>();

        foreach (var d in recentDocs)
            activities.Add(new RecentActivityItemDto(
                Id: d.Id.ToString(),
                DocumentId: d.Id.ToString(),
                Type: "document",
                Title: "Document uploaded",
                Description: d.FileName,
                At: d.UploadedAt
            ));

        foreach (var q in recentQuestions)
        {
            var desc = q.Document != null ? q.Document.FileName + " — " : "";
            desc += q.QuestionText.Length > 50 ? q.QuestionText[..50] + "…" : q.QuestionText;
            activities.Add(new RecentActivityItemDto(
                Id: q.Id.ToString(),
                DocumentId: q.DocumentId.ToString(),
                Type: "question",
                Title: "AI consultation",
                Description: desc,
                At: q.AskedAt
            ));
        }

        var ordered = activities
            .OrderByDescending(a => a.At)
            .Take(count)
            .ToList();

        return Ok(ordered);
    }
}
