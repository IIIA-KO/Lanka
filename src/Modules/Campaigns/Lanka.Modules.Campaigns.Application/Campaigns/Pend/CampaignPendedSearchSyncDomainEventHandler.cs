using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Pend;

internal sealed class CampaignPendedSearchSyncDomainEventHandler
    : DomainEventHandler<CampaignPendedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private static readonly string[] _tags = ["campaign", "pending"];

    public CampaignPendedSearchSyncDomainEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        CampaignPendedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<CampaignResponse> result = await this._sender.Send(
            new GetCampaignQuery(domainEvent.CampaignId.Value),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetCampaignQuery), CampaignErrors.NotFound);
        }

        var metadata = new Dictionary<string, object>
        {
            { "status", "pending" },
            { "offerId", result.Value.OfferId },
            { "clientId", result.Value.ClientId },
            { "creatorId", result.Value.CreatorId },
            { "scheduledOnUtc", result.Value.ScheduledOnUtc }
        };

        var integrationEvent = new CampaignSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.CampaignId.Value,
            SearchSyncOperation.Create,
            title: result.Value.Name,
            content: result.Value.Description,
            _tags,
            metadata
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
