using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Suggestions;

internal sealed class GetSearchSuggestionsQueryHandler
    : IQueryHandler<GetSearchSuggestionsQuery, IReadOnlyCollection<string>>
{
    private readonly ISearchService _searchService;

    public GetSearchSuggestionsQueryHandler(ISearchService searchService)
    {
        this._searchService = searchService;
    }

    public async Task<Result<IReadOnlyCollection<string>>> Handle(
        GetSearchSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        SearchableItemType? itemType = request.ItemType.HasValue
                                       && Enum.IsDefined(typeof(SearchableItemType), request.ItemType.Value)
            ? (SearchableItemType)request.ItemType.Value
            : null;

        IReadOnlyCollection<string> suggestions = await this._searchService.GetSearchSuggestionsAsync(
            request.Query,
            itemType,
            request.Limit,
            cancellationToken
        );

        return suggestions.ToArray();
    }
}
