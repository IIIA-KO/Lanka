using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Domain.Users.DomainEvents;
using Lanka.Modules.Users.IntegrationEvents;

namespace Lanka.Modules.Users.Application.Users.Delete;

internal sealed class UserDeletedDomainEventHandler
    : DomainEventHandler<UserDeletedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public UserDeletedDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(UserDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await this._eventBus.PublishAsync(
            new UserDeletedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId.Value
            ),
            cancellationToken
        );
    }
}
