namespace back.Data.Entities;

public class LegalReference
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string ArticleOrSection { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public DateTime ExtractedAt { get; set; }
}
