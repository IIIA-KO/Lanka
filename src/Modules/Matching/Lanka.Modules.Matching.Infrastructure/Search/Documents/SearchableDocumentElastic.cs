namespace Lanka.Modules.Matching.Infrastructure.Search.Documents;

public sealed class SearchableDocumentElastic
{
    public Guid Id { get; set; }
    public Guid SourceEntityId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, object> Metadata { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
}


