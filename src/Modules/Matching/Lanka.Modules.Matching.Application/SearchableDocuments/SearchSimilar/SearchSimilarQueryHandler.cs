using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.SearchableDocuments.Search;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries;
using Lanka.Modules.Matching.Domain.Searches.SearchResults;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.SearchSimilar;

internal sealed class SearchSimilarQueryHandler : IQueryHandler<SearchSimilarQuery, SearchSimilarResponse>
{
    private readonly ISearchService _searchService;

    public SearchSimilarQueryHandler(ISearchService searchService)
    {
        this._searchService = searchService;
    }

    public async Task<Result<SearchSimilarResponse>> Handle(
        SearchSimilarQuery request,
        CancellationToken cancellationToken
    )
    {
        Result<SearchQuery> searchQueryResult = SearchQuery.Create(
            text: string.Empty,
            enableFuzzySearch: false,
            enableSynonyms: false,
            fuzzyDistance: 0.8,
            request.ItemTypes,
            request.NumericFilters,
            request.FacetFilters,
            request.CreatedAfter,
            request.CreatedBefore,
            request.OnlyActive,
            request.Page,
            request.Size
        );

        if (searchQueryResult.IsFailure)
        {
            return Result.Failure<SearchSimilarResponse>(searchQueryResult.Error);
        }

        SearchResult searchResults =
            await this._searchService.SearchSimilarAsync(
                request.SourceItemId,
                request.SourceType,
                searchQueryResult.Value,
                cancellationToken
            );

        var response = new SearchSimilarResponse(
            searchResults.Items.Select(SearchResultResponse.FromSearchResultItem).ToList(),
            searchResults.TotalCount,
            searchResults.Page,
            searchResults.Size,
            searchResults.SearchDuration,
            searchResults.Facets.ToDictionary(
                kvp => kvp.Key,
                IReadOnlyCollection<FacetResult> (kvp) =>
                    kvp.Value
                        .Select(f => new FacetResult(f.Value, f.Count))
                        .ToList()
            )
        );

        return response;
    }
}
