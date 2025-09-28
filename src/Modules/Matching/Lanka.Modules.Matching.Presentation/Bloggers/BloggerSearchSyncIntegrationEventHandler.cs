using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.IntegrationEvents.Bloggers;
using Lanka.Modules.Matching.Domain.SearchableItems;    
using Serilog;

namespace Lanka.Modules.Matching.Presentation.Bloggers;

internal sealed class BloggerSearchSyncIntegrationEventHandler
    : IntegrationEventHandler<BloggerSearchSyncIntegrationEvent>
{
    private readonly SearchSyncIntegrationEventService _syncSearchService;
    private readonly ILogger _logger;

    public BloggerSearchSyncIntegrationEventHandler(
        SearchSyncIntegrationEventService syncSearchService,
        ILogger logger
    )
    {
        this._syncSearchService = syncSearchService;
        this._logger = logger;
    }

    public override async Task Handle(
        BloggerSearchSyncIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.Information(
            "Processing blogger search sync: BloggerId={EntityId}, Operation={Operation}, Title={Title}",
            integrationEvent.EntityId,
            integrationEvent.Operation,
            integrationEvent.Title
        );

        Result result = await this._syncSearchService.Handle(integrationEvent, SearchableItemType.Blogger);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(BloggerSearchSyncIntegrationEventHandler), result.Error);
        }

        this._logger.Information(
            "Successfully processed blogger search sync: BloggerId={EntityId}, Operation={Operation}",
            integrationEvent.EntityId,
            integrationEvent.Operation
        );
    }
}
