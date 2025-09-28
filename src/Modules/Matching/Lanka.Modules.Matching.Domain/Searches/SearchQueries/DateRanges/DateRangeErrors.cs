using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.DateRanges;

public static class DateRangeErrors
{
    public static Error Invalid =>
        Error.Validation(
            "DateRange.Invalid",
            "Created after date must be before created before date."
        );
}
