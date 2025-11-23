using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Presentation.IntegrationEvents.Services;
using Serilog;

namespace Lanka.Modules.Matching.Presentation.IntegrationEvents.Handlers;

internal sealed class CampaignSearchSyncIntegrationEventHandler
    : IntegrationEventHandler<CampaignSearchSyncIntegrationEvent>
{
    private readonly SearchSyncIntegrationEventService _syncSearchService;
    private readonly ILogger _logger;

    public CampaignSearchSyncIntegrationEventHandler(
        SearchSyncIntegrationEventService syncSearchService,
        ILogger logger
    )
    {
        this._syncSearchService = syncSearchService;
        this._logger = logger;
    }

    public override async Task Handle(
        CampaignSearchSyncIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.Information(
            "Processing campaign search sync: CampaignId={EntityId}, Operation={Operation}, Title={Title}",
            integrationEvent.EntityId,
            integrationEvent.Operation,
            integrationEvent.Title
        );

        Result result = await this._syncSearchService.Handle(integrationEvent, SearchableItemType.Campaign);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(CampaignSearchSyncIntegrationEvent), result.Error);
        }

        this._logger.Information(
            "Successfully processed campaign search sync: CampaignId={EntityId}, Operation={Operation}",
            integrationEvent.EntityId,
            integrationEvent.Operation
        );
    }
}
