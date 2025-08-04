using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Domain.Users.DomainEvents;
using Lanka.Modules.Users.IntegrationEvents;

namespace Lanka.Modules.Users.Application.Users.Login;

public class UserLoggedInDomainEventHandler
    : DomainEventHandler<UserLoggedInDomainEvent>
{
    private readonly IEventBus _eventBus;

    public UserLoggedInDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        UserLoggedInDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await this._eventBus.PublishAsync(
            new UserLoggedInIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId.Value,
                domainEvent.LastLoggedInAtUtc
            ),
            cancellationToken
        );
    }
}
