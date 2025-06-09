using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Domain.Users.DomainEvents;
using Lanka.Modules.Users.IntegrationEvents;

namespace Lanka.Modules.Users.Application.Instagram.Link;

public class InstagramAccountLinkedDomainEventHandler
    : DomainEventHandler<InstagramAccountLinkedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public InstagramAccountLinkedDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(InstagramAccountLinkedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await this._eventBus.PublishAsync(
            new InstagramAccountLinkedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId.Value,
                domainEvent.Code
            ),
            cancellationToken
        );
    }
}
