using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.IntegrationEvents.Reviews;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Serilog;

namespace Lanka.Modules.Matching.Presentation.Reviews;

internal sealed class ReviewSearchSyncIntegrationEventHandler
    : IntegrationEventHandler<ReviewSearchSyncIntegrationEvent>
{
    private readonly SearchSyncIntegrationEventService _syncSearchService;
    private readonly ILogger _logger;

    public ReviewSearchSyncIntegrationEventHandler(
        SearchSyncIntegrationEventService syncSearchService,
        ILogger logger
    )
    {
        this._syncSearchService = syncSearchService;
        this._logger = logger;
    }

    public override async Task Handle(
        ReviewSearchSyncIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.Information(
            "Processing review search sync: ReviewId={EntityId}, Operation={Operation}, Title={Title}",
            integrationEvent.EntityId,
            integrationEvent.Operation,
            integrationEvent.Title
        );

        Result result = await this._syncSearchService.Handle(integrationEvent, SearchableItemType.Review);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(ReviewSearchSyncIntegrationEventHandler), result.Error);
        }

        this._logger.Information(
            "Successfully processed review search sync: ReviewId={EntityId}, Operation={Operation}",
            integrationEvent.EntityId,
            integrationEvent.Operation
        );
    }
}
