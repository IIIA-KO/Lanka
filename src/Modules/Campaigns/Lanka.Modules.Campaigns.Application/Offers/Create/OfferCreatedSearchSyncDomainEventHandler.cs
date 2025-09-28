using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Campaigns.Application.Offers.GetOffer;
using Lanka.Modules.Campaigns.Domain.Offers.DomainEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Offers;
using MediatR;

namespace Lanka.Modules.Campaigns.Application.Offers.Create;

internal sealed class OfferCreatedSearchSyncDomainEventHandler
    : DomainEventHandler<OfferCreatedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;
    private static readonly string[] _tags = ["offer", "service"];

    public OfferCreatedSearchSyncDomainEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        OfferCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<OfferResponse> result = await this._sender.Send(
            new GetOfferQuery(domainEvent.OfferId.Value),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetOfferQuery), result.Error);
        }

        var integrationEvent = new OfferSearchSyncIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            entityId: domainEvent.OfferId.Value,
            SearchSyncOperation.Create,
            title: result.Value.Name,
            content: result.Value.Description,
            _tags,
            metadata: new Dictionary<string, object>
            {
                { "Price", result.Value.PriceAmount }
            }
        );

        await this._eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
