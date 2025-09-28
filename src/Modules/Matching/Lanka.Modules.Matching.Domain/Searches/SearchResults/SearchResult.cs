using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.Facets;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchResultItems;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults;

public sealed record SearchResult
{
    public IReadOnlyCollection<SearchResultItem> Items { get; init; }
    public long TotalCount { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
    public TimeSpan SearchDuration { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<Facet>> Facets { get; init; }

    private SearchResult(
        IReadOnlyCollection<SearchResultItem> items,
        long totalCount,
        int page,
        int size,
        TimeSpan searchDuration,
        IReadOnlyDictionary<string, IReadOnlyCollection<Facet>> facets
    )
    {
        this.Items = items;
        this.TotalCount = totalCount;
        this.Page = page;
        this.Size = size;
        this.SearchDuration = searchDuration;
        this.Facets = facets;
    }

    public static Result<SearchResult> Create(
        IEnumerable<SearchResultItem>? items,
        long totalCount,
        int page,
        int size,
        TimeSpan searchDuration,
        IDictionary<string, IReadOnlyCollection<Facet>>? facets = null
    )
    {
        Result validationResult = ValidateSearchResult(totalCount, page, size);

        if (validationResult.IsFailure)
        {
            return Result.Failure<SearchResult>(validationResult.Error);
        }

        return new SearchResult(
            items?.ToList() ?? [],
            Math.Max(0, totalCount),
            page,
            size,
            searchDuration,
            facets?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? []
        );
    }

    public bool HasResults => this.Items.Count > 0;

    public int PageCount =>
        this.Size > 0 ? CalculatePageCount(this.TotalCount, this.Size) : 0;

    private static int CalculatePageCount(long totalCount, int size) =>
        (int)Math.Ceiling((double)totalCount / size);

    public bool HasNextPage => this.Page < this.PageCount;

    public bool HasPreviousPage => this.Page > 1;

    public static SearchResult Empty(int page, int size) =>
        new([], 0, page, size, TimeSpan.Zero, new Dictionary<string, IReadOnlyCollection<Facet>>());

    private static Result ValidateSearchResult(long totalCount, int page, int size)
    {
        if (totalCount < 0)
        {
            return SearchResultErrors.InvalidTotalCount;
        }

        if (page < 1)
        {
            return SearchResultErrors.InvalidPage;
        }

        if (size < 1)
        {
            return SearchResultErrors.InvalidSize;
        }

        return Result.Success();
    }
}
