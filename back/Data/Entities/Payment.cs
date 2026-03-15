using System.ComponentModel.DataAnnotations;

namespace back.Data.Entities;

public class Payment
{
    [Key]
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(50)]
    public string Plan { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [MaxLength(50)]
    public string Status { get; set; } = "completed";

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "google_pay";

    [MaxLength(500)]
    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PlanExpiresAt { get; set; }
}
