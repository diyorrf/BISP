using back.Data.Repos.Interfaces;
using back.Extensions;
using back.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AccountController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

        return Ok(new AccountDto(
            Email: user.Email,
            FullName: user.FullName,
            TokensRemaining: user.TokensRemaining,
            Plan: user.Plan ?? "Free",
            EmailConfirmed: user.EmailConfirmed
        ));
    }
}
