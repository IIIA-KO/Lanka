using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents;

public sealed class UserDeletedIntegrationEvent : IntegrationEvent
{
    public UserDeletedIntegrationEvent(
        Guid id, 
        DateTime occurredOnUtc,
        Guid userId
    ) 
        : base(id, occurredOnUtc)
    {
        this.UserId = userId;
    }
    
    public Guid UserId { get; init; }
}
