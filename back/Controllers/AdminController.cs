using back.Data;
using back.Data.Entities;
using back.Models.DTOs;
using back.Services.Parser;
using back.Services.Regulatory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IRegulatoryMatchingService _matchingService;
    private readonly IDocumentParserService _parserService;
    private readonly ILogger<AdminController> _logger;
    private readonly string _storagePath;

    public AdminController(
        AppDbContext db,
        IRegulatoryMatchingService matchingService,
        IDocumentParserService parserService,
        ILogger<AdminController> logger,
        IWebHostEnvironment env)
    {
        _db = db;
        _matchingService = matchingService;
        _parserService = parserService;
        _logger = logger;
        _storagePath = Path.Combine(env.ContentRootPath, "Storage");
        Directory.CreateDirectory(_storagePath);
    }

    // ── Stats ──────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken ct)
    {
        var totalUsers = await _db.Users.CountAsync(ct);
        var activeUsers = await _db.Users.CountAsync(u => u.IsActive, ct);
        var totalDocuments = await _db.Documents.CountAsync(ct);
        var totalQuestions = await _db.Questions.CountAsync(ct);
        var totalAlerts = await _db.RegulatoryAlerts.CountAsync(ct);
        var unreadAlerts = await _db.RegulatoryAlerts.CountAsync(a => !a.IsRead, ct);
        var totalPayments = await _db.Payments.CountAsync(ct);
        var totalRevenue = await _db.Payments.SumAsync(p => p.Amount, ct);

        var planBreakdown = await _db.Users
            .GroupBy(u => u.Plan)
            .Select(g => new { Plan = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Plan, x => x.Count, ct);

        var regTotal = await _db.RegulatoryUpdates.CountAsync(ct);
        var regPending = await _db.RegulatoryUpdates.CountAsync(r => !r.IsProcessed, ct);

        return Ok(new AdminStatsDto(
            totalUsers, activeUsers, totalDocuments, totalQuestions,
            totalAlerts, unreadAlerts, totalPayments, totalRevenue,
            planBreakdown, regTotal, regPending
        ));
    }

    // ── Users ──────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers(CancellationToken ct)
    {
        var users = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto(
                u.Id, u.Email, u.FullName, u.IsActive, u.EmailConfirmed,
                u.Plan, u.TokensRemaining, u.CreatedAt, u.LastLoginAt,
                _db.Documents.Count(d => d.UserId == u.Id),
                _db.Questions.Count(q => q.UserId == u.Id),
                u.UserRoles.Select(ur => ur.Role.Code).ToList()
            ))
            .ToListAsync(ct);

        return Ok(users);
    }

    [HttpGet("users/{id:long}")]
    public async Task<ActionResult<AdminUserDetailDto>> GetUser(long id, CancellationToken ct)
    {
        var u = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (u == null) return NotFound();

        var docs = await _db.Documents
            .Where(d => d.UserId == id)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new AdminDocumentDto(
                d.Id, d.UserId, u.Email, d.FileName, d.ContentType,
                d.SizeInBytes, d.UploadedAt,
                d.Questions.Count, d.LegalReferences.Count
            ))
            .ToListAsync(ct);

        var payments = await _db.Payments
            .Where(p => p.UserId == id)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminPaymentDto(
                p.Id, p.UserId, u.Email, p.Plan, p.Amount,
                p.Currency, p.Status, p.PaymentMethod,
                p.TransactionId, p.CreatedAt, p.PlanExpiresAt
            ))
            .ToListAsync(ct);

        var dto = new AdminUserDetailDto(
            u.Id, u.Email, u.FullName, u.IsActive, u.EmailConfirmed,
            u.Plan, u.TokensRemaining, u.CreatedAt, u.LastLoginAt,
            u.UpdatedAt, u.LastTokenResetAt,
            docs.Count,
            await _db.Questions.CountAsync(q => q.UserId == id, ct),
            u.UserRoles.Select(ur => ur.Role.Code).ToList(),
            docs, payments
        );

        return Ok(dto);
    }

    [HttpPut("users/{id:long}/plan")]
    public async Task<IActionResult> UpdateUserPlan(long id, [FromBody] UpdateUserPlanDto dto, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user == null) return NotFound();

        var validPlans = new[] { "Free", "Pro", "Enterprise" };
        if (!validPlans.Contains(dto.Plan))
            return BadRequest("Invalid plan. Must be Free, Pro, or Enterprise.");

        user.Plan = dto.Plan;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = $"Plan updated to {dto.Plan}" });
    }

    [HttpPut("users/{id:long}/toggle-active")]
    public async Task<IActionResult> ToggleUserActive(long id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { isActive = user.IsActive });
    }

    [HttpPut("users/{id:long}/tokens")]
    public async Task<IActionResult> SetUserTokens(long id, [FromBody] UpdateUserTokensDto dto, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user == null) return NotFound();

        user.TokensRemaining = dto.Tokens;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { tokensRemaining = user.TokensRemaining });
    }

    // ── Documents ──────────────────────────────────────────

    [HttpGet("documents")]
    public async Task<ActionResult<List<AdminDocumentDto>>> GetDocuments(CancellationToken ct)
    {
        var docs = await _db.Documents
            .Include(d => d.Questions)
            .Include(d => d.LegalReferences)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new AdminDocumentDto(
                d.Id, d.UserId,
                _db.Users.Where(u => u.Id == d.UserId).Select(u => u.Email).FirstOrDefault(),
                d.FileName, d.ContentType, d.SizeInBytes, d.UploadedAt,
                d.Questions.Count, d.LegalReferences.Count
            ))
            .ToListAsync(ct);

        return Ok(docs);
    }

    [HttpGet("documents/{id:guid}/references")]
    public async Task<ActionResult<AdminDocumentReferencesDto>> GetDocumentReferences(Guid id, CancellationToken ct)
    {
        var doc = await _db.Documents.FindAsync([id], ct);
        if (doc == null) return NotFound();

        var refs = await _db.LegalReferences
            .Where(r => r.DocumentId == id)
            .OrderBy(r => r.ExtractedAt)
            .Select(r => new AdminLegalReferenceDto(
                r.Id, r.Title, r.ArticleOrSection, r.Jurisdiction, r.RawText, r.ExtractedAt
            ))
            .ToListAsync(ct);

        return Ok(new AdminDocumentReferencesDto(id, doc.FileName, refs));
    }

    // ── Regulatory Updates ─────────────────────────────────

    [HttpGet("regulatory-updates")]
    public async Task<ActionResult<List<AdminRegulatoryUpdateDto>>> GetRegulatoryUpdates(CancellationToken ct)
    {
        var updates = await _db.RegulatoryUpdates
            .Include(r => r.Alerts)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AdminRegulatoryUpdateDto(
                r.Id, r.Title, r.Description, r.LawIdentifier,
                r.SourceUrl, r.EffectiveDate, r.PublishedAt,
                r.CreatedAt, r.IsProcessed, r.Alerts.Count
            ))
            .ToListAsync(ct);

        return Ok(updates);
    }

    [HttpPost("regulatory-updates")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AdminRegulatoryUpdateDto>> CreateRegulatoryUpdate(
        [FromForm] string title,
        [FromForm] string lawIdentifier,
        [FromForm] string? description,
        [FromForm] string? sourceUrl,
        [FromForm] DateTime? effectiveDate,
        [FromForm] DateTime? publishedAt,
        IFormFile? file,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(lawIdentifier))
            return BadRequest("Title and LawIdentifier are required");

        string? content = null;
        string? storedFileName = null;

        if (file != null && file.Length > 0)
        {
            if (!_parserService.IsSupported(file.ContentType))
                return BadRequest($"File type '{file.ContentType}' is not supported. Please upload PDF, DOCX, or TXT files.");

            content = await _parserService.ExtractTextAsync(file, ct);

            var fileId = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName);
            storedFileName = $"reg_{fileId}{extension}";
            var filePath = Path.Combine(_storagePath, storedFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);
        }

        var entity = new RegulatoryUpdate
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description ?? string.Empty,
            LawIdentifier = lawIdentifier,
            SourceUrl = sourceUrl,
            Content = content,
            StoredFileName = storedFileName,
            EffectiveDate = effectiveDate.HasValue
                ? DateTime.SpecifyKind(effectiveDate.Value, DateTimeKind.Utc)
                : null,
            PublishedAt = publishedAt.HasValue
                ? DateTime.SpecifyKind(publishedAt.Value, DateTimeKind.Utc)
                : DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        _db.RegulatoryUpdates.Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Regulatory update {Id} created with {HasFile}", entity.Id, file != null ? "file" : "no file");

        return CreatedAtAction(nameof(GetRegulatoryUpdates), new AdminRegulatoryUpdateDto(
            entity.Id, entity.Title, entity.Description, entity.LawIdentifier,
            entity.SourceUrl, entity.EffectiveDate, entity.PublishedAt,
            entity.CreatedAt, entity.IsProcessed, 0
        ));
    }

    [HttpPost("regulatory-updates/{id:guid}/process")]
    public async Task<ActionResult<ProcessResultDto>> ProcessRegulatoryUpdate(Guid id, CancellationToken ct)
    {
        var update = await _db.RegulatoryUpdates.FindAsync([id], ct);
        if (update == null) return NotFound();

        if (update.IsProcessed)
            return Ok(new ProcessResultDto(id, 0, "This update has already been processed."));

        var alertsCreated = await _matchingService.MatchAndCreateAlertsAsync(update, ct);

        update.IsProcessed = true;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Admin manually processed regulatory update {Id}: {Alerts} alerts created", id, alertsCreated);

        return Ok(new ProcessResultDto(id, alertsCreated, $"Processing complete. {alertsCreated} alert(s) created."));
    }

    [HttpDelete("regulatory-updates/{id:guid}")]
    public async Task<IActionResult> DeleteRegulatoryUpdate(Guid id, CancellationToken ct)
    {
        var update = await _db.RegulatoryUpdates.FindAsync([id], ct);
        if (update == null) return NotFound();

        _db.RegulatoryUpdates.Remove(update);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ── Payments ───────────────────────────────────────────

    [HttpGet("payments")]
    public async Task<ActionResult<List<AdminPaymentDto>>> GetPayments(CancellationToken ct)
    {
        var payments = await _db.Payments
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminPaymentDto(
                p.Id, p.UserId,
                _db.Users.Where(u => u.Id == p.UserId).Select(u => u.Email).FirstOrDefault(),
                p.Plan, p.Amount, p.Currency, p.Status,
                p.PaymentMethod, p.TransactionId, p.CreatedAt, p.PlanExpiresAt
            ))
            .ToListAsync(ct);

        return Ok(payments);
    }
}
