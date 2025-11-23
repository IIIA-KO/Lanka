using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;

namespace Lanka.Modules.Matching.Application.Index.Create;

internal sealed class CreateDocumentCommandHandler : ICommandHandler<CreateDocumentCommand>
{
    private readonly ISearchIndexService _indexService;

    public CreateDocumentCommandHandler(ISearchIndexService indexService)
    {
        this._indexService = indexService;
    }

    public async Task<Result> Handle(
        CreateDocumentCommand request,
        CancellationToken cancellationToken
    )
    {
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

        SearchDocument document = createResult.Value;
        
        Result indexResult = await this._indexService.IndexDocumentAsync(document, cancellationToken);
        
        return indexResult.IsFailure ? indexResult : Result.Success();
    }
}
