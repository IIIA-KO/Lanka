using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.IntegrationEvents;

namespace Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;

public class InstagramAccountDataFetchedDomainEventHandler
    : DomainEventHandler<InstagramAccountDataFetchedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public InstagramAccountDataFetchedDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        InstagramAccountDataFetchedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await this._eventBus.PublishAsync(
            new InstagramAccountDataFetchedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId.Value,
                domainEvent.Username,
                domainEvent.ProviderId
            ),
            cancellationToken
        );
    }
}
