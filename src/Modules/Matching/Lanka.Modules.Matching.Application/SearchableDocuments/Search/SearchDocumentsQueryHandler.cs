using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries;
using Lanka.Modules.Matching.Domain.Searches.SearchResults;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Search;

internal sealed class SearchDocumentsQueryHandler : IQueryHandler<SearchDocumentsQuery, SearchDocumentsResponse>
{
    private readonly ISearchService _searchService;

    public SearchDocumentsQueryHandler(ISearchService searchService)
    {
        this._searchService = searchService;
    }

    public async Task<Result<SearchDocumentsResponse>> Handle(
        SearchDocumentsQuery request,
        CancellationToken cancellationToken
    )
    {
        Result<SearchQuery> searchQueryResult = SearchQuery.Create(
            request.Query,
            request.EnableFuzzySearch,
            request.EnableSynonyms,
            request.FuzzyDistance,
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
            return Result.Failure<SearchDocumentsResponse>(searchQueryResult.Error);
        }

        SearchResult searchResult = await this._searchService.SearchAsync(searchQueryResult.Value, cancellationToken);

        var items = searchResult.Items
            .Select(SearchResultResponse.FromSearchResultItem)
            .ToList();

        var facets = searchResult.Facets
            .ToDictionary(
                kvp => kvp.Key, IReadOnlyCollection<FacetResult> (kvp) => kvp.Value
                    .Select(f => new FacetResult(f.Value, f.Count))
                    .ToList()
            );

        SearchDocumentsResponse response = new(
            items,
            searchResult.TotalCount,
            searchResult.Page,
            searchResult.Size,
            searchResult.SearchDuration,
            facets
        );

        return response;
    }
}
