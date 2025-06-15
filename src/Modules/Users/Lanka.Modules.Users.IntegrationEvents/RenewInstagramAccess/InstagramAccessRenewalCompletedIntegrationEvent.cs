using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

public sealed class InstagramAccessRenewalCompletedIntegrationEvent : IntegrationEvent
{
    public InstagramAccessRenewalCompletedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
    }

    public Guid UserId { get; init; }
}
