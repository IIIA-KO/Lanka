using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.SearchableDocuments.Titles;

public static class TitleErrors
{
    public static Error Empty =>
        Error.Validation("Title.Empty", "Title cannot be empty.");

    public static Error TooLong(int maxLength) =>
        Error.Validation("Title.TooLong", $"Title cannot exceed {maxLength} characters.");
}


