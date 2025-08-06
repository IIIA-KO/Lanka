using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

public class InstagramRenewalAccessFailureCleanedUpIntegrationEvent : IntegrationEvent
{
    public InstagramRenewalAccessFailureCleanedUpIntegrationEvent(
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
