using Lanka.Common.Domain;
using Lanka.Common.IntegrationEvents;
using Lanka.Modules.Matching.Application.SearchableDocuments.Index;
using Lanka.Modules.Matching.Application.SearchableDocuments.Remove;
using Lanka.Modules.Matching.Application.SearchableDocuments.Update;
using Lanka.Modules.Matching.Domain.SearchableItems;
using MediatR;
using Serilog;

namespace Lanka.Modules.Matching.Presentation;

public sealed class SearchSyncIntegrationEventService
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    public SearchSyncIntegrationEventService(ISender sender, ILogger logger)
    {
        this._sender = sender;
        this._logger = logger;
    }

    public async Task<Result> Handle(
        SearchSyncIntegrationEvent integrationEvent,
        SearchableItemType itemType
    )
    {
        Result result = await (integrationEvent.Operation switch
        {
            SearchSyncOperation.Create => this.HandleCreate(integrationEvent, itemType),
            SearchSyncOperation.Update => this.HandleUpdate(integrationEvent, itemType),
            SearchSyncOperation.Delete => this.HandleDelete(integrationEvent, itemType),
            _ => throw new ArgumentOutOfRangeException(
                nameof(integrationEvent),
                integrationEvent.Operation,
                "Unknown search sync operation"
            )
        });

        if (result.IsFailure)
        {
            this._logger.Error(
                "Failed to handle search sync operation {Operation} for {EntityType} with ID {EntityId}: {Error}",
                integrationEvent.Operation,
                itemType,
                integrationEvent.EntityId,
                result.Error
            );
        }

        return result;
    }

    private async Task<Result> HandleCreate(
        SearchSyncIntegrationEvent integrationEvent,
        SearchableItemType itemType
    )
    {
        var command = new IndexDocumentCommand(
            integrationEvent.EntityId,
            itemType,
            integrationEvent.Title ?? string.Empty,
            integrationEvent.Content ?? string.Empty,
            integrationEvent.Tags ?? [],
            integrationEvent.Metadata
        );

        return await this._sender.Send(command);
    }

    private async Task<Result> HandleUpdate(
        SearchSyncIntegrationEvent integrationEvent,
        SearchableItemType itemType
    )
    {
        var command = new UpdateSearchableDocumentContentCommand(
            integrationEvent.EntityId,
            itemType,
            integrationEvent.Title ?? string.Empty,
            integrationEvent.Content ?? string.Empty,
            integrationEvent.Tags ?? [],
            integrationEvent.Metadata
        );

        return await this._sender.Send(command);
    }

    private async Task<Result> HandleDelete(
        SearchSyncIntegrationEvent integrationEvent,
        SearchableItemType itemType
    )
    {
        var command = new RemoveDocumentCommand(
            integrationEvent.EntityId,
            itemType
        );

        return await this._sender.Send(command);
    }
}
