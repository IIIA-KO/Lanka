using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.IntegrationEvents;

namespace Lanka.Modules.Analytics.Application.Instagram.DeleteAccountData;

internal sealed class InstagramAccountDeletedSearchSyncDomainEventHandler
    : DomainEventHandler<InstagramAccountDeletedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public InstagramAccountDeletedSearchSyncDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        InstagramAccountDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var integrationEvent = new InstagramAccountSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.InstagramAccountId.Value,
            SearchSyncOperation.Delete
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

