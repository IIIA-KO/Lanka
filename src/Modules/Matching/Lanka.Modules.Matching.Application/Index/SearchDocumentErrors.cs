using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Application.Index;

public static class SearchDocumentErrors
{
    public static readonly Error EmptyDocument = Error.Validation(
        "SearchDocument.EmptyDocument",
        "At least Title or Content must be provided for a searchable document."
    );

    public static Error TitleTooLong(int actualLength, int maxLength) => Error.Validation(
        "SearchDocument.TitleTooLong",
        $"Title length ({actualLength}) exceeds maximum allowed length ({maxLength})."
    );

    public static Error ContentTooLong(int actualLength, int maxLength) => Error.Validation(
        "SearchDocument.ContentTooLong",
        $"Content length ({actualLength}) exceeds maximum allowed length ({maxLength})."
    );
}
