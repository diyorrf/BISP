using back.Data.Entities;
using back.Data.Repos.Interfaces;
using back.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers;

[ApiController]
[Route("api/regulatory-updates")]
[Authorize]
public class RegulatoryUpdatesController : ControllerBase
{
    private readonly IRegulatoryUpdateRepository _updateRepository;

    public RegulatoryUpdatesController(IRegulatoryUpdateRepository updateRepository)
    {
        _updateRepository = updateRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RegulatoryUpdateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegulatoryUpdateDto>>> GetAll(CancellationToken ct)
    {
        var updates = await _updateRepository.GetAllAsync(ct);
        var dtos = updates.Select(u => new RegulatoryUpdateDto(
            u.Id, u.Title, u.Description, u.LawIdentifier,
            u.SourceUrl, u.EffectiveDate, u.PublishedAt, u.IsProcessed
        ));
        return Ok(dtos);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RegulatoryUpdateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegulatoryUpdateDto>> Create(
        [FromBody] CreateRegulatoryUpdateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.LawIdentifier))
            return BadRequest("Title and LawIdentifier are required");

        var entity = new RegulatoryUpdate
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            LawIdentifier = request.LawIdentifier,
            SourceUrl = request.SourceUrl,
            EffectiveDate = request.EffectiveDate,
            PublishedAt = request.PublishedAt,
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        await _updateRepository.AddAsync(entity, ct);

        var dto = new RegulatoryUpdateDto(
            entity.Id, entity.Title, entity.Description, entity.LawIdentifier,
            entity.SourceUrl, entity.EffectiveDate, entity.PublishedAt, entity.IsProcessed
        );

        return CreatedAtAction(nameof(GetAll), dto);
    }
}
