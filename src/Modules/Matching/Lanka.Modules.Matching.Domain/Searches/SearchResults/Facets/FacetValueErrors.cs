using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults.Facets;

public static class FacetValueErrors
{
    public static Error EmptyValue =>
        Error.Validation("FacetValue.EmptyValue", "Facet value cannot be empty.");

    public static Error InvalidCount =>
        Error.Validation("FacetValue.InvalidCount", "Count cannot be negative.");
}



