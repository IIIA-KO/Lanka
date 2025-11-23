using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;

namespace Lanka.Modules.Matching.Application.Index.Update;

internal sealed class UpdateSearchableDocumentContentCommandHandler
    : ICommandHandler<UpdateSearchableDocumentContentCommand>
{
    private readonly ISearchIndexService _indexService;

    public UpdateSearchableDocumentContentCommandHandler(ISearchIndexService indexService)
    {
        this._indexService = indexService;
    }

    public async Task<Result> Handle(
        UpdateSearchableDocumentContentCommand request,
        CancellationToken cancellationToken
    )
    {
        // Create and validate document
        Result<SearchDocument> createResult = SearchDocument.Create(
            request.SourceEntityId,
            request.Type,
            request.Title,
            request.Content,
            request.Tags,
            request.Metadata
        );

        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        // Use update/upsert for better performance and safety
        Result updateResult = await this._indexService.UpdateDocumentAsync(
            createResult.Value,
            cancellationToken
        );

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        return Result.Success();
    }
}
