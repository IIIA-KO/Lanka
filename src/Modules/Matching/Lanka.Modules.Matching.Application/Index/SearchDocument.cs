using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.Index;

public sealed class SearchDocument
{
    public const int TitleMaxLength = 200;
    public const int ContentMaxLength = 5000;

    public required Guid Id { get; init; }
    public required Guid SourceEntityId { get; init; }
    public required SearchableItemType Type { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required IReadOnlyCollection<string> Tags { get; init; }
    public required IReadOnlyDictionary<string, object> Metadata { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public required bool IsActive { get; init; }

    public static Result<SearchDocument> Create(
        Guid sourceEntityId,
        SearchableItemType type,
        string? title,
        string? content,
        IEnumerable<string>? tags,
        IDictionary<string, object>? metadata
    )
    {
        string normalizedTitle = title?.Trim() ?? string.Empty;
        string normalizedContent = content?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedTitle) && string.IsNullOrWhiteSpace(normalizedContent))
        {
            return Result.Failure<SearchDocument>(SearchDocumentErrors.EmptyDocument);
        }

        if (normalizedTitle.Length > TitleMaxLength)
        {
            return Result.Failure<SearchDocument>(
                SearchDocumentErrors.TitleTooLong(normalizedTitle.Length, TitleMaxLength)
            );
        }

        if (normalizedContent.Length > ContentMaxLength)
        {
            return Result.Failure<SearchDocument>(
                SearchDocumentErrors.ContentTooLong(normalizedContent.Length, ContentMaxLength)
            );
        }

        Guid documentId = GenerateDeterministicId(sourceEntityId, type);

        return new SearchDocument
        {
            Id = documentId,
            SourceEntityId = sourceEntityId,
            Type = type,
            Title = normalizedTitle,
            Content = normalizedContent,
            Tags = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList() ?? [],
            Metadata = metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [],
            LastUpdated = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    private static Guid GenerateDeterministicId(Guid sourceEntityId, SearchableItemType type)
    {
        byte[] sourceBytes = sourceEntityId.ToByteArray();
        byte[] typeBytes = BitConverter.GetBytes((int)type);
        
        for (int i = 0; i < 4; i++)
        {
            sourceBytes[i] ^= typeBytes[i];
        }
        
        return new Guid(sourceBytes);
    }
}
