using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Presentation.IntegrationEvents.Services;
using Serilog;

namespace Lanka.Modules.Matching.Presentation.IntegrationEvents.Handlers;

internal sealed class SearchSyncIntegrationEventHandler
    : IntegrationEventHandler<SearchSyncIntegrationEvent>
{
    private static readonly Dictionary<string, SearchableItemType> ItemTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Blogger"] = SearchableItemType.Blogger,
        ["Campaign"] = SearchableItemType.Campaign,
        ["Offer"] = SearchableItemType.Offer,
        ["Pact"] = SearchableItemType.Pact,
        ["Review"] = SearchableItemType.Review,
        ["InstagramAccount"] = SearchableItemType.InstagramAccount,
    };

    private readonly SearchSyncIntegrationEventService _syncSearchService;
    private readonly ILogger _logger;

    public SearchSyncIntegrationEventHandler(
        SearchSyncIntegrationEventService syncSearchService,
        ILogger logger)
    {
        this._syncSearchService = syncSearchService;
        this._logger = logger;
    }

    public override async Task Handle(
        SearchSyncIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        if (!ItemTypeMap.TryGetValue(integrationEvent.ItemType, out SearchableItemType itemType))
        {
            this._logger.Warning(
                "Unknown item type '{ItemType}' for entity {EntityId}, skipping search sync",
                integrationEvent.ItemType,
                integrationEvent.EntityId);
            return;
        }

        this._logger.Debug(
            "Processing search sync: EntityId={EntityId}, ItemType={ItemType}, Operation={Operation}",
            integrationEvent.EntityId,
            itemType,
            integrationEvent.Operation);

        Result result = await this._syncSearchService.Handle(integrationEvent, itemType);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(SearchSyncIntegrationEventHandler), result.Error);
        }
    }
}
