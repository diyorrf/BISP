namespace back.Data.Entities;

public class DocumentTopic
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string Topic { get; set; } = string.Empty;
    public DateTime ExtractedAt { get; set; }
}
