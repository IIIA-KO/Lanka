using Lanka.Common.IntegrationEvents;

namespace Lanka.Modules.Campaigns.IntegrationEvents.Offers;

public sealed class OfferSearchSyncIntegrationEvent
    : SearchSyncIntegrationEvent
{
    public OfferSearchSyncIntegrationEvent(
        Guid id,
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
