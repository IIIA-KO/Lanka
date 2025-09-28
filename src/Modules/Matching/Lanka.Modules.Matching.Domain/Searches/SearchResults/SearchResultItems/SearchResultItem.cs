using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchHighlights;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchResultItems;

public sealed record SearchResultItem
{
    public Guid ItemId { get; init; }
    public SearchableItemType Type { get; init; }
    public double RelevanceScore { get; init; }
    public IReadOnlyCollection<SearchHighlight> Highlights { get; init; }
    public IReadOnlyDictionary<string, object> Metadata { get; init; }

    private SearchResultItem(
        Guid itemId,
        SearchableItemType type,
        double relevanceScore,
        IReadOnlyCollection<SearchHighlight> highlights,
        IReadOnlyDictionary<string, object> metadata
    )
    {
        this.ItemId = itemId;
        this.Type = type;
        this.RelevanceScore = relevanceScore;
        this.Highlights = highlights;
        this.Metadata = metadata;
    }

    public static Result<SearchResultItem> Create(
        Guid itemId,
        string? type,
        double relevanceScore,
        IEnumerable<SearchHighlight>? highlights = null,
        IDictionary<string, object>? metadata = null
    )
    {
        Result validationResult = ValidateSearchResultItem(relevanceScore);

        if (validationResult.IsFailure)
        {
            return Result.Failure<SearchResultItem>(validationResult.Error);
        }

        return new SearchResultItem(
            itemId,
            Enum.TryParse(type, out SearchableItemType enumType) 
                ? enumType 
                : SearchableItemType.Unknown,
            relevanceScore,
            highlights?.ToList() ?? [],
            metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? []
        );
    }

    public T? GetMetadataValue<T>(string key) where T : class
    {
        return this.Metadata.TryGetValue(key, out object? value) ? value as T : null;
    }

    public T GetMetadataValue<T>(string key, T defaultValue) where T : struct
    {
        return this.Metadata.TryGetValue(key, out object? value) && value is T typedValue
            ? typedValue
            : defaultValue;
    }

    private static Result ValidateSearchResultItem(double relevanceScore)
    {
        if (relevanceScore < 0)
        {
            return SearchResultItemErrors.InvalidRelevanceScore;
        }

        return Result.Success();
    }
}



