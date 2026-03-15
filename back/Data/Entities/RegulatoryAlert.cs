namespace back.Data.Entities;

public class RegulatoryAlert
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid RegulatoryUpdateId { get; set; }
    public RegulatoryUpdate RegulatoryUpdate { get; set; } = null!;
    public Guid LegalReferenceId { get; set; }
    public LegalReference LegalReference { get; set; } = null!;
    public bool IsRead { get; set; }
    public bool IsDismissed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
