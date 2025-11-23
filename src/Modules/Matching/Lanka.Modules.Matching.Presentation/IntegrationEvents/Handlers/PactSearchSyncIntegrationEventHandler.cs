using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.IntegrationEvents.Pacts;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Presentation.IntegrationEvents.Services;
using Serilog;

namespace Lanka.Modules.Matching.Presentation.IntegrationEvents.Handlers;

internal sealed class PactSearchSyncIntegrationEventHandler
    : IntegrationEventHandler<PactSearchSyncIntegrationEvent>
{
    private readonly SearchSyncIntegrationEventService _syncSearchService;
    private readonly ILogger _logger;

    public PactSearchSyncIntegrationEventHandler(
        SearchSyncIntegrationEventService syncSearchService,
        ILogger logger
    )
    {
        this._syncSearchService = syncSearchService;
        this._logger = logger;
    }

    public override async Task Handle(
        PactSearchSyncIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.Information(
            "Processing pact search sync: PactId={EntityId}, Operation={Operation}, Title={Title}",
            integrationEvent.EntityId,
            integrationEvent.Operation,
            integrationEvent.Title
        );

        Result result = await this._syncSearchService.Handle(integrationEvent, SearchableItemType.Pact);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(PactSearchSyncIntegrationEventHandler), result.Error);
        }

        this._logger.Information(
            "Successfully processed pact search sync: PactId={EntityId}, Operation={Operation}",
            integrationEvent.EntityId,
            integrationEvent.Operation
        );
    }
}
