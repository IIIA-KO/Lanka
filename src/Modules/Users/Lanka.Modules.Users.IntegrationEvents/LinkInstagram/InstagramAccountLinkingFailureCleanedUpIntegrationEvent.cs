using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

public sealed class InstagramAccountLinkingFailureCleanedUpIntegrationEvent : IntegrationEvent
{
    public InstagramAccountLinkingFailureCleanedUpIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid correlationId,
        string reason
    ) : base(id, occurredOnUtc)
    {
        this.CorrelationId = correlationId;
        this.Reason = reason;
    }

    public Guid CorrelationId { get; }
    public string Reason { get; }
}
