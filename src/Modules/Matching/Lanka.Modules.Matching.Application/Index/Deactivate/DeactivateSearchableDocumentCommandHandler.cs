using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;

namespace Lanka.Modules.Matching.Application.Index.Deactivate;

internal sealed class DeactivateSearchableDocumentCommandHandler 
    : ICommandHandler<DeactivateSearchableDocumentCommand>
{
    private readonly ISearchIndexService _indexService;

    public DeactivateSearchableDocumentCommandHandler(ISearchIndexService indexService)
    {
        this._indexService = indexService;
    }

    public async Task<Result> Handle(DeactivateSearchableDocumentCommand request, CancellationToken cancellationToken)
    {
        Result result = await this._indexService.DeactivateDocumentsBySourceEntityAsync(
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
