using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.IntegrationEvents;

namespace Lanka.Modules.Analytics.Application.Instagram.RefreshToken;

internal sealed class InstagramAccountUpdatedSearchSyncDomainEventHandler
    : DomainEventHandler<InstagramAccountDataRenewedDomainEvent>
{
    private readonly IEventBus _eventBus;
    private static readonly string[] _tags = ["instagram", "social-media"];

    public InstagramAccountUpdatedSearchSyncDomainEventHandler(IEventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        InstagramAccountDataRenewedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var integrationEvent = new InstagramAccountSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.InstagramAccountId.Value,
            SearchSyncOperation.Update,
            title: domainEvent.Username,
            content:
            $"Instagram account with {domainEvent.FollowersCount} followers and {domainEvent.MediaCount} posts",
            _tags,
            metadata: new Dictionary<string, object>
            {
                { "UserId", domainEvent.UserId.ToString() },
                { "Username", domainEvent.Username },
                { "FollowersCount", domainEvent.FollowersCount },
                { "MediaCount", domainEvent.MediaCount }
            }
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
