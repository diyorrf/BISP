using back.Data;
using back.Data.Entities;
using back.Data.Repos.Interfaces;
using back.Extensions;
using back.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PaymentsController> _logger;

    public static readonly Dictionary<string, PlanDto> Plans = new()
    {
        ["Free"] = new PlanDto(
            "Free", "Free", "Get started with basic legal analysis",
            0m, "USD", 25_000, 5,
            new[] { "5 AI prompts per day", "Up to 5 documents", "Basic contract scanning", "Email support" }
        ),
        ["Pro"] = new PlanDto(
            "Pro", "Pro", "For professionals who need more power",
            9.99m, "USD", 100_000, 50,
            new[] { "20 AI prompts per day", "Up to 50 documents", "Advanced contract scanning", "Conversation history", "Priority support" }
        ),
        ["Enterprise"] = new PlanDto(
            "Enterprise", "Enterprise", "Unlimited access for teams and firms",
            29.99m, "USD", 500_000, -1,
            new[] { "Unlimited AI prompts", "Unlimited documents", "Full regulatory alerts", "Custom analysis", "Dedicated support", "API access" }
        )
    };

    public PaymentsController(AppDbContext db, IUserRepository userRepository, ILogger<PaymentsController> logger)
    {
        _db = db;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("plans")]
    [ProducesResponseType(typeof(PlanDto[]), StatusCodes.Status200OK)]
    public IActionResult GetPlans()
    {
        return Ok(Plans.Values.ToArray());
    }

    [HttpPost("process")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        if (!Plans.TryGetValue(request.Plan, out var plan))
            return BadRequest(new PaymentResultDto(false, "Invalid plan selected", request.Plan, 0, null));

        if (plan.Price == 0)
            return BadRequest(new PaymentResultDto(false, "Cannot purchase the Free plan", request.Plan, 0, null));

        if (string.IsNullOrWhiteSpace(request.PaymentToken))
            return BadRequest(new PaymentResultDto(false, "Payment token is required", request.Plan, 0, null));

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            return Unauthorized();

        var expiresAt = DateTime.UtcNow.AddMonths(1);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Plan = request.Plan,
            Amount = plan.Price,
            Currency = plan.Currency,
            Status = "completed",
            PaymentMethod = "google_pay",
            TransactionId = request.TransactionId ?? Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            PlanExpiresAt = expiresAt
        };

        await _db.Payments.AddAsync(payment, ct);

        user.Plan = request.Plan;
        user.TokensRemaining = plan.DailyTokens;
        user.LastTokenResetAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} upgraded to {Plan} via Google Pay. Transaction: {TxId}",
            userId, request.Plan, payment.TransactionId);

        return Ok(new PaymentResultDto(
            true,
            $"Successfully upgraded to {request.Plan} plan!",
            request.Plan,
            user.TokensRemaining,
            expiresAt
        ));
    }

    [HttpGet("history")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentHistoryDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentHistory(CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var payments = await _db.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentHistoryDto(
                p.Id, p.Plan, p.Amount, p.Currency, p.Status, p.CreatedAt, p.PlanExpiresAt
            ))
            .ToListAsync(ct);

        return Ok(payments);
    }
}
