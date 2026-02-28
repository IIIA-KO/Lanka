using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;

namespace Lanka.Modules.Campaigns.Application.ChangeCapture;

internal sealed class EntityChangeCapturedDomainEventHandler
    : DomainEventHandler<EntityChangeCapturedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public EntityChangeCapturedDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        EntityChangeCapturedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var integrationEvent = new SearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.EntityId,
            (SearchSyncOperation)domainEvent.Operation,
            domainEvent.Title,
            domainEvent.Content,
            domainEvent.Tags,
            domainEvent.Metadata,
            itemType: domainEvent.ItemType);

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
