using Lanka.Common.Application.EventBus;

namespace Lanka.Common.IntegrationEvents;

public abstract class SearchSyncIntegrationEvent : IntegrationEvent
{
    protected SearchSyncIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid entityId,
        SearchSyncOperation operation,
        string? title = null,
        string? content = null,
        IReadOnlyCollection<string>? tags = null,
        IDictionary<string, object>? metadata = null
    ) : base(id, occurredOnUtc)
    {
        this.EntityId = entityId;
        this.Operation = operation;
        this.Title = title;
        this.Content = content;
        this.Tags = tags;
        this.Metadata = metadata;
    }

    public Guid EntityId { get; }
    public SearchSyncOperation Operation { get; }
    public string? Title { get; }
    public string? Content { get; }
    public IReadOnlyCollection<string>? Tags { get; }
    public IDictionary<string, object>? Metadata { get; }
}
