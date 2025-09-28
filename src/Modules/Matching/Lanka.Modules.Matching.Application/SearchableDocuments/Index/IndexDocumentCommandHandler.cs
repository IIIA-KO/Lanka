using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Domain.SearchableDocuments;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Index;

internal sealed class IndexDocumentCommandHandler : ICommandHandler<IndexDocumentCommand>
{
    private readonly ISearchIndexService _indexService;

    public IndexDocumentCommandHandler(ISearchIndexService indexService)
    {
        this._indexService = indexService;
    }

    public async Task<Result> Handle(
        IndexDocumentCommand request,
        CancellationToken cancellationToken
    )
    {
        Result<SearchableDocument> createResult = SearchableDocument.Create(
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
        
        Result indexResult = await this._indexService.IndexDocumentAsync(createResult.Value, cancellationToken);
        
        if (indexResult.IsFailure)
        {
            return indexResult;
        }

        return Result.Success();
    }
}
