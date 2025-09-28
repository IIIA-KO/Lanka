using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchResultItems;

public static class SearchResultItemErrors
{
    public static Error InvalidRelevanceScore =>
        Error.Validation("SearchResultItem.InvalidRelevanceScore", "Relevance score cannot be negative.");
}
