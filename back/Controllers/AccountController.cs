using back.Data;
using back.Data.Repos.Interfaces;
using back.Extensions;
using back.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _db;

    public AccountController(IUserRepository userRepository, AppDbContext db)
    {
        _userRepository = userRepository;
        _db = db;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountDto>> GetMe(CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            return Unauthorized();

        DateTime? planExpiry = null;
        if (user.Plan != "Free")
        {
            planExpiry = await _db.Payments
                .Where(p => p.UserId == userId && p.Status == "completed")
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.PlanExpiresAt)
                .FirstOrDefaultAsync(ct);
        }

        return Ok(new AccountDto(
            Email: user.Email,
            FullName: user.FullName,
            TokensRemaining: user.TokensRemaining,
            Plan: user.Plan ?? "Free",
            EmailConfirmed: user.EmailConfirmed,
            PlanExpiresAt: planExpiry
        ));
    }
}
