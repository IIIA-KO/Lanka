using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Domain.Reviews.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Reviews;

namespace Lanka.Modules.Campaigns.Application.Reviews.Delete;

internal sealed class ReviewDeletedSearchSyncDomainEventHandler
    : DomainEventHandler<ReviewDeletedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public ReviewDeletedSearchSyncDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        ReviewDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var integrationEvent = new ReviewSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.ReviewId.Value,
            SearchSyncOperation.Delete
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

