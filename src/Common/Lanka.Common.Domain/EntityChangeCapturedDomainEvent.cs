namespace Lanka.Common.Domain;

public sealed class EntityChangeCapturedDomainEvent : DomainEvent
{
    public EntityChangeCapturedDomainEvent(
        Guid entityId,
        string itemType,
        int operation,
        string? title = null,
        string? content = null,
        IReadOnlyCollection<string>? tags = null,
        IDictionary<string, object>? metadata = null)
    {
        this.EntityId = entityId;
        this.ItemType = itemType;
        this.Operation = operation;
        this.Title = title;
        this.Content = content;
        this.Tags = tags;
        this.Metadata = metadata;
    }

    public Guid EntityId { get; init; }

    public string ItemType { get; init; }

    /// <summary>
    /// 1 = Create, 2 = Update, 3 = Delete.
    /// Maps to <see cref="Microsoft.EntityFrameworkCore.EntityState"/>: Added, Modified, Deleted.
    /// </summary>
    public int Operation { get; init; }

    public string? Title { get; init; }

    public string? Content { get; init; }

    public IReadOnlyCollection<string>? Tags { get; init; }

    public IDictionary<string, object>? Metadata { get; init; }
}
