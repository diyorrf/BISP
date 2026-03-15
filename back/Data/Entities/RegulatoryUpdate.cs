namespace back.Data.Entities;

public class RegulatoryUpdate
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LawIdentifier { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
    public ICollection<RegulatoryAlert> Alerts { get; set; } = new List<RegulatoryAlert>();
}
