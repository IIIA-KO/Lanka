using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Campaigns.MarkAsDone;

internal sealed class CampaignMarkedAsDoneDomainEventHandler
    : IDomainEventHandler<CampaignMarkedAsDoneDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;

    public CampaignMarkedAsDoneDomainEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public async Task Handle(CampaignMarkedAsDoneDomainEvent notification, CancellationToken cancellationToken)
    {
        Result<CampaignResponse> result =
            await this._sender.Send(new GetCampaignQuery(notification.CampaignId.Value), cancellationToken);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetCampaignQuery), CampaignErrors.NotFound);
        }

        await this._eventBus.PublishAsync(
            new CampaignMarkedAsDoneIntegrationEvent(
                notification.Id,
                notification.OcurredOnUtc,
                notification.CampaignId.Value,
                result.Value.OfferId,
                result.Value.ClientId,
                result.Value.CreatorId
            ),
            cancellationToken
        );
    }
}
