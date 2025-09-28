using Lanka.Modules.Matching.Application.SearchableDocuments.Search;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.SearchSimilar;

public sealed record SearchSimilarResponse(
    IReadOnlyCollection<SearchResultResponse> Results,
    long TotalCount,
    int Page,
    int Size,
    TimeSpan SearchDuration,
    IReadOnlyDictionary<string, IReadOnlyCollection<FacetResult>> Facets
);

