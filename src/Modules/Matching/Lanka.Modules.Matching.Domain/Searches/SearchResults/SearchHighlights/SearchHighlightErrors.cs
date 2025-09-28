using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchHighlights;

public static class SearchHighlightErrors
{
    public static Error EmptyFieldName =>
        Error.Validation("SearchHighlight.EmptyFieldName", "Field name cannot be empty.");

    public static Error EmptyFragments =>
        Error.Validation("SearchHighlight.EmptyFragments", "Fragments cannot be empty.");
}



