using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Domain.Offers.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Offers;

namespace Lanka.Modules.Campaigns.Application.Offers.Delete;

internal sealed class OfferDeletedSearchSyncDomainEventHandler
    : DomainEventHandler<OfferDeletedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public OfferDeletedSearchSyncDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        OfferDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var integrationEvent = new OfferSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.OfferId.Value,
            SearchSyncOperation.Delete
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

