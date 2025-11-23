using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;

namespace Lanka.Modules.Matching.Application.Index.Activate;

internal sealed class ActivateSearchableDocumentCommandHandler 
    : ICommandHandler<ActivateSearchableDocumentCommand>
{
    private readonly ISearchIndexService _indexService;

    public ActivateSearchableDocumentCommandHandler(ISearchIndexService indexService)
    {
        this._indexService = indexService;
    }

    public async Task<Result> Handle(ActivateSearchableDocumentCommand request, CancellationToken cancellationToken)
    {
        Result result = await this._indexService.ActivateDocumentsBySourceEntityAsync(
            request.SourceEntityId,
            request.Type,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return result;
        }

        return Result.Success();
    }
}
