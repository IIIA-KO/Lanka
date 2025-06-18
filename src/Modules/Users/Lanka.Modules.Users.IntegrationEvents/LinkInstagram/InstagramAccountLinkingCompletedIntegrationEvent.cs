using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

public sealed class InstagramAccountLinkingCompletedIntegrationEvent : IntegrationEvent
{
    public InstagramAccountLinkingCompletedIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc,
        Guid userId
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
    }
    
    public Guid UserId { get; }
}
