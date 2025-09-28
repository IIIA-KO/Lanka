using Lanka.Common.IntegrationEvents;

namespace Lanka.Modules.Analytics.IntegrationEvents;

public sealed class InstagramAccountSearchSyncIntegrationEvent
    : SearchSyncIntegrationEvent
{
    public InstagramAccountSearchSyncIntegrationEvent(Guid id,
        DateTime occurredOnUtc,
        Guid entityId,
        SearchSyncOperation operation,
        string? title = null,
        string? content = null,
        IReadOnlyCollection<string>? tags = null,
        IDictionary<string, object>? metadata = null
    ) : base(id, occurredOnUtc, entityId, operation, title, content, tags, metadata)
    {
    }
}
