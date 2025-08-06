using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

public class InstagramRenewalFailedIntegrationEvent : IntegrationEvent
{
    public InstagramRenewalFailedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string reason
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Reason = reason;
    }

    public Guid UserId { get; }

    public string Reason { get; set; }
}
