using Lanka.Common.Application.EventBus;

namespace Lanka.Modules.Users.IntegrationEvents;

public sealed class UserLoggedInIntegrationEvent : IntegrationEvent
{
    public UserLoggedInIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        DateTimeOffset lastLoggedInAtUtc
    ) : base(id, occurredOnUtc)
    {
        this.UserId = userId;
        this.LastLoggedInAtUtc = lastLoggedInAtUtc;
    }
    
    public Guid UserId { get; init; }
    
    public DateTimeOffset LastLoggedInAtUtc { get; init; }
}
