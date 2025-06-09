using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents;

public sealed class InstagramAccountLinkingStartedIntegrationEvent : IntegrationEvent
{
    public InstagramAccountLinkingStartedIntegrationEvent(
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
