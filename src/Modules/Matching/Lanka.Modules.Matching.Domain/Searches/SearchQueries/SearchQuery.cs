using System.Globalization;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.DateRanges;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.FuzzyDistances;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.Paginations;
using Lanka.Modules.Matching.Domain.Searches.SearchQueries.SearchTexts;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries;

public sealed record SearchQuery
{
    public string Text { get; init; }
    public bool EnableFuzzySearch { get; init; }
    public bool EnableSynonyms { get; init; }
    public double FuzzyDistance { get; init; }
    public IReadOnlyCollection<SearchableItemType> ItemTypes { get; init; }
    public IReadOnlyDictionary<string, object> NumericFilters { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> FacetFilters { get; init; }
    public DateTimeOffset? CreatedAfter { get; init; }
    public DateTimeOffset? CreatedBefore { get; init; }
    public bool OnlyActive { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }

    private SearchQuery(
        string text,
        bool enableFuzzySearch,
        bool enableSynonyms,
        double fuzzyDistance,
        IReadOnlyCollection<SearchableItemType> itemTypes,
        IReadOnlyDictionary<string, object> numericFilters,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> facetFilters,
        DateTimeOffset? createdAfter,
        DateTimeOffset? createdBefore,
        bool onlyActive,
        int page,
        int size
    )
    {
        this.Text = text;
        this.EnableFuzzySearch = enableFuzzySearch;
        this.EnableSynonyms = enableSynonyms;
        this.FuzzyDistance = fuzzyDistance;
        this.ItemTypes = itemTypes;
        this.NumericFilters = numericFilters;
        this.FacetFilters = facetFilters;
        this.CreatedAfter = createdAfter;
        this.CreatedBefore = createdBefore;
        this.OnlyActive = onlyActive;
        this.Page = page;
        this.Size = size;
    }

    public static Result<SearchQuery> Create(
        string text,
        bool enableFuzzySearch = true,
        bool enableSynonyms = true,
        double fuzzyDistance = 0.8,
        string? itemTypes = null,
        IDictionary<string, object>? numericFilters = null,
        IDictionary<string, IReadOnlyCollection<string>>? facetFilters = null,
        DateTimeOffset? createdAfter = null,
        DateTimeOffset? createdBefore = null,
        bool onlyActive = true,
        int page = 1,
        int size = 20
    )
    {
        Result<SearchText> searchTextResult = SearchText.Create(text);
        Result<FuzzyDistance> fuzzyDistanceResult = FuzzyDistances.FuzzyDistance.Create(fuzzyDistance);
        Result<Pagination> paginationResult = Pagination.Create(page, size);
        Result<DateRange> dateRangeResult = DateRange.Create(createdAfter, createdBefore);

        IReadOnlyCollection<SearchableItemType>? itemTypesList = SearchableItemTypes(itemTypes);

        Result<SearchQuery> validationResult = new ValidationBuilder()
            .Add(searchTextResult)
            .Add(fuzzyDistanceResult)
            .Add(paginationResult)
            .Add(dateRangeResult)
            .Build(() =>
                new SearchQuery(
                    text: searchTextResult.Value.Value,
                    enableFuzzySearch,
                    enableSynonyms,
                    fuzzyDistanceResult.Value.Value,
                    itemTypesList,
                    numericFilters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [],
                    facetFilters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [],
                    createdAfter,
                    createdBefore,
                    onlyActive,
                    page,
                    size
                )
            );

        return validationResult;
    }

    private static IReadOnlyCollection<SearchableItemType> SearchableItemTypes(string? itemTypes)
    {
        IReadOnlyCollection<SearchableItemType> itemTypesList = string.IsNullOrEmpty(itemTypes)
            ? []
            : itemTypes.Split(',')
                .Where(s => int.TryParse(s, out _) && Enum.IsDefined(typeof(SearchableItemType),
                    int.Parse(s, CultureInfo.InvariantCulture)))
                .Select(s => (SearchableItemType)int.Parse(s, CultureInfo.InvariantCulture))
                .ToList();
        return itemTypesList;
    }

    public static SearchQuery Simple(string text) => Create(text).Value;

    public static SearchQuery WithFilters(
        string text,
        string? itemTypes = null,
        IDictionary<string, object>? numericFilters = null,
        IDictionary<string, IReadOnlyCollection<string>>? facetFilters = null
    ) => Create(text, itemTypes: itemTypes, numericFilters: numericFilters, facetFilters: facetFilters).Value;

    public static SearchQuery WithDateRange(
        string text,
        DateTimeOffset? createdAfter = null,
        DateTimeOffset? createdBefore = null
    ) => Create(text, createdAfter: createdAfter, createdBefore: createdBefore).Value;

    public SearchQuery WithItemTypes(params SearchableItemType[] types) =>
        this with { ItemTypes = types.ToList() };

    public SearchQuery WithPagination(int page, int size) =>
        this with { Page = page, Size = size };
}
