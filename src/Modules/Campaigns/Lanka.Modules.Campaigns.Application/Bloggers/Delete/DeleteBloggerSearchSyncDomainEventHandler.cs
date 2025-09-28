using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Delete;

internal sealed class DeleteBloggerSearchSyncDomainEventHandler
    : DomainEventHandler<BloggerDeletedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public DeleteBloggerSearchSyncDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        BloggerDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var integrationEvent = new BloggerSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.BloggerId.Value,
            SearchSyncOperation.Delete
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
