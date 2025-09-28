namespace Lanka.Modules.Matching.Domain.SearchableItems;

public sealed class SearchableItem
{
    public Guid Id { get; private set; }
    public SearchableItemType Type { get; private set; }

    public string Title { get; private set; }

    public string Content { get; private set; }

    public IReadOnlyCollection<string> Tags { get; private set; }

    public DateTime LastUpdated { get; private set; }

    public bool IsActive { get; private set; }

    private SearchableItem() { }

    public SearchableItem(
        Guid id,
        SearchableItemType type,
        string title,
        string content,
        IEnumerable<string> tags
    )
    {
        this.Id = id;
        this.Type = type;
        this.Title = title;
        this.Content = content;
        this.Tags = tags.ToList();
        this.LastUpdated = DateTime.UtcNow;
        this.IsActive = true;
    }

    public void Deactivate()
    {
        this.IsActive = false;
        this.LastUpdated = DateTime.UtcNow;
    }

    public void UpdateContent(string title, string content, IEnumerable<string> tags)
    {
        this.Title = title;
        this.Content = content;
        this.Tags = tags.ToList();
        this.LastUpdated = DateTime.UtcNow;
    }
}
