using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults;

public static class SearchResultErrors
{
    public static Error InvalidTotalCount =>
        Error.Validation("SearchResult.InvalidTotalCount", "Total count cannot be negative.");

    public static Error InvalidPage =>
        Error.Validation("SearchResult.InvalidPage", "Page must be greater than 0.");

    public static Error InvalidSize =>
        Error.Validation("SearchResult.InvalidSize", "Size must be greater than 0.");
}



