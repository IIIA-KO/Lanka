using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.IntegrationEvents.Offers;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Presentation.IntegrationEvents.Services;
using Serilog;

namespace Lanka.Modules.Matching.Presentation.IntegrationEvents.Handlers;

internal sealed class OfferSearchSyncIntegrationEventHandler
    : IntegrationEventHandler<OfferSearchSyncIntegrationEvent>
{
    private readonly SearchSyncIntegrationEventService _syncSearchService;
    private readonly ILogger _logger;

    public OfferSearchSyncIntegrationEventHandler(
        SearchSyncIntegrationEventService syncSearchService,
        ILogger logger
    )
    {
        this._syncSearchService = syncSearchService;
        this._logger = logger;
    }

    public override async Task Handle(
        OfferSearchSyncIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.Information(
            "Processing offer search sync: OfferId={EntityId}, Operation={Operation}, Title={Title}",
            integrationEvent.EntityId,
            integrationEvent.Operation,
            integrationEvent.Title
        );

        Result result = await this._syncSearchService.Handle(integrationEvent, SearchableItemType.Offer);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(OfferSearchSyncIntegrationEventHandler), result.Error);
        }

        this._logger.Information(
            "Successfully processed offer search sync: OfferId={EntityId}, Operation={Operation}",
            integrationEvent.EntityId,
            integrationEvent.Operation
        );
    }
}
