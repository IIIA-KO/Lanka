using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.IntegrationEvents;

namespace Lanka.Modules.Analytics.Application.Instagram.RefreshToken;

internal sealed class RenewInstagramAccountDataDomainEventHandler
    : DomainEventHandler<InstagramAccountDataRenewedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public RenewInstagramAccountDataDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        InstagramAccountDataRenewedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await this._eventBus.PublishAsync(
            new InstagramAccountDataRenewedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId.Value,
                domainEvent.Username,
                domainEvent.FollowersCount,
                domainEvent.MediaCount,
                domainEvent.ProviderId
            ),
            cancellationToken
        );
    }
}
