using back.Extensions;
using back.Models.DTOs;
using back.Services.Regulatory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    private long? UserId => User.GetUserId();

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RegulatoryAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegulatoryAlertDto>>> GetAlerts(
        [FromQuery] bool? isRead, CancellationToken ct)
    {
        if (UserId is not { } userId) return Unauthorized();
        var alerts = await _alertService.GetAlertsAsync(userId, isRead, ct);
        return Ok(alerts);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken ct)
    {
        if (UserId is not { } userId) return Unauthorized();
        var count = await _alertService.GetUnreadCountAsync(userId, ct);
        return Ok(count);
    }

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        if (UserId is not { } userId) return Unauthorized();
        var result = await _alertService.MarkAsReadAsync(id, userId, ct);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        if (UserId is not { } userId) return Unauthorized();
        await _alertService.MarkAllAsReadAsync(userId, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/dismiss")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct)
    {
        if (UserId is not { } userId) return Unauthorized();
        var result = await _alertService.DismissAsync(id, userId, ct);
        return result ? NoContent() : NotFound();
    }
}
