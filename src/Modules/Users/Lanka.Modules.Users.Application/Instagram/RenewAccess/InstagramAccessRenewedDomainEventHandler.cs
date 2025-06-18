using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Domain.Users.DomainEvents;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;

namespace Lanka.Modules.Users.Application.Instagram.RenewAccess;

internal sealed class InstagramAccessRenewedDomainEventHandler
    : DomainEventHandler<InstagramAccessRenewedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public InstagramAccessRenewedDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        InstagramAccessRenewedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await this._eventBus.PublishAsync(
            new InstagramAccessRenewedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId.Value,
                domainEvent.Code
            ),
            cancellationToken
        );
    }
}
