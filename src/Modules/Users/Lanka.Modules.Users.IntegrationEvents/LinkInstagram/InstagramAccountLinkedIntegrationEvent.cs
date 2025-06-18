using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents.LinkInstagram;

public sealed class InstagramAccountLinkedIntegrationEvent : IntegrationEvent
{
    public InstagramAccountLinkedIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc,
        Guid userId,
        string code
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.Code = code;
    }
    
    public Guid UserId { get; init; }
    
    public string Code { get; init; }
}
