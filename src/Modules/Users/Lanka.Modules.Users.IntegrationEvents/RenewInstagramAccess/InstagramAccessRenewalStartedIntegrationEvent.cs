using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

public sealed class InstagramAccessRenewalStartedIntegrationEvent : IntegrationEvent
{
    public InstagramAccessRenewalStartedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string code
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Code = code;
    }

    public Guid UserId { get; }

    public string Code { get; init; }
}
