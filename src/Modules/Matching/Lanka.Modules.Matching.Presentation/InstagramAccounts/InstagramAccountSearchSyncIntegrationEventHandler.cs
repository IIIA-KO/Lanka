using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.IntegrationEvents;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Serilog;

namespace Lanka.Modules.Matching.Presentation.InstagramAccounts;

internal sealed class InstagramAccountSearchSyncIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccountSearchSyncIntegrationEvent>
{
    private readonly SearchSyncIntegrationEventService _syncSearchService;
    private readonly ILogger _logger;

    public InstagramAccountSearchSyncIntegrationEventHandler(
        SearchSyncIntegrationEventService syncSearchService,
        ILogger logger
    )
    {
        this._syncSearchService = syncSearchService;
        this._logger = logger;
    }

    public override async Task Handle(
        InstagramAccountSearchSyncIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.Information(
            "Processing instagram account search sync: InstagramAccountId={EntityId}, Operation={Operation}, Title={Title}",
            integrationEvent.EntityId,
            integrationEvent.Operation,
            integrationEvent.Title
        );

        Result result = await this._syncSearchService.Handle(integrationEvent, SearchableItemType.InstagramAccount);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(InstagramAccountSearchSyncIntegrationEventHandler), result.Error);
        }

        this._logger.Information(
            "Successfully processed instagram account  search sync: InstagramAccountId={EntityId}, Operation={Operation}",
            integrationEvent.EntityId,
            integrationEvent.Operation
        );
    }
}
