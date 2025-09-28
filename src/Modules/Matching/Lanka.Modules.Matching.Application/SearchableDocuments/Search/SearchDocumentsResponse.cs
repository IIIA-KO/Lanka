using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchResultItems;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Search;

public sealed record SearchDocumentsResponse(
    IReadOnlyCollection<SearchResultResponse> Results,
    long TotalCount,
    int Page,
    int Size,
    TimeSpan SearchDuration,
    IReadOnlyDictionary<string, IReadOnlyCollection<FacetResult>> Facets
);

public sealed record SearchResultResponse(
    Guid ItemId,
    SearchableItemType Type,
    double RelevanceScore,
    IReadOnlyCollection<SearchHighlightResponse> Highlights,
    IReadOnlyDictionary<string, object> Metadata
)
{
    public static SearchResultResponse FromSearchResultItem(SearchResultItem result)
    {
        return new SearchResultResponse(
            result.ItemId,
            result.Type,
            result.RelevanceScore,
            result.Highlights.Select(h => new SearchHighlightResponse(h.FieldName, h.Fragments)).ToList(),
            result.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
}

public sealed record SearchHighlightResponse(
    string FieldName,
    IReadOnlyCollection<string> Fragments
);

public sealed record FacetResult(
    string Value,
    long Count
);

