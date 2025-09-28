using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.SearchableDocuments.Contents;

public static class ContentErrors
{
    public static Error Empty =>
        Error.Validation("Content.Empty", "Content cannot be empty.");

    public static Error TooLong(int maxLength) =>
        Error.Validation("Content.TooLong", $"Content cannot exceed {maxLength} characters.");
}


