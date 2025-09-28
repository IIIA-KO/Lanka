using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchQueries.SearchTexts;

public static class SearchTextErrors
{
    public static Error EmptyText =>
        Error.Validation(
            "SearchText.EmptyText",
            "Search text cannot be empty."
        );

    public static Error TextTooLong(int maxLength) =>
        Error.Validation(
            "SearchText.TextTooLong",
            $"Search text cannot exceed {maxLength} characters."
        );
}
