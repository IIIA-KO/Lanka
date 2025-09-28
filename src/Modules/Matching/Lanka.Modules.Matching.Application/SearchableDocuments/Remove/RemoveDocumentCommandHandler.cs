using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Remove;

internal sealed class RemoveDocumentCommandHandler : ICommandHandler<RemoveDocumentCommand>
{
    private readonly ISearchIndexService _indexService;

    public RemoveDocumentCommandHandler(ISearchIndexService indexService)
    {
        this._indexService = indexService;
    }

    public async Task<Result> Handle(RemoveDocumentCommand request, CancellationToken cancellationToken)
    {
        Result removeResult = await this._indexService.RemoveDocumentsBySourceEntityAsync(
            request.SourceEntityId,
            request.Type,
            cancellationToken
        );

        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        return Result.Success();
    }
}
