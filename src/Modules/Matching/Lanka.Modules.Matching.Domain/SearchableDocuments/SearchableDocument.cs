using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.SearchableDocuments.Contents;
using Lanka.Modules.Matching.Domain.SearchableDocuments.Titles;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Domain.SearchableDocuments;

public sealed class SearchableDocument : Entity<SearchableDocumentId>
{
    public SearchableItemType Type { get; private set; }
    public Title Title { get; private set; }
    public Content Content { get; private set; }
    public IReadOnlyCollection<string> Tags { get; private set; }
    public IReadOnlyDictionary<string, object> Metadata { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }
    public bool IsActive { get; private set; }
    public Guid SourceEntityId { get; private set; }

    private SearchableDocument()
    {
    }

    private SearchableDocument(
        SearchableDocumentId id,
        Guid sourceEntityId,
        SearchableItemType type,
        Title title,
        Content content,
        IEnumerable<string> tags,
        IDictionary<string, object>? metadata
    ) : base(id)
    {
        this.SourceEntityId = sourceEntityId;
        this.Type = type;
        this.Title = title;
        this.Content = content;
        this.Tags = tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        this.Metadata = metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [];
        this.LastUpdated = DateTime.UtcNow;
        this.IsActive = true;
    }

    public static Result<SearchableDocument> Create(
        Guid sourceEntityId,
        SearchableItemType type,
        string title,
        string content,
        IEnumerable<string> tags,
        IDictionary<string, object>? metadata = null
    )
    {
        Result<(Title, Content)> validationResult = Validate(title, content);

        if (validationResult.IsFailure)
        {
            return Result.Failure<SearchableDocument>(validationResult.Error);
        }

        (Title _title, Content _content) = validationResult.Value;

        var document = new SearchableDocument(
            SearchableDocumentId.New(),
            sourceEntityId,
            type,
            _title,
            _content,
            tags,
            metadata
        );

        return document;
    }

    private static Result<(Title, Content)> Validate(
        string title,
        string content
    )
    {
        Result<Title> titleResult = Title.Create(title);
        Result<Content> contentResult = Content.Create(content);

        return new ValidationBuilder()
            .Add(titleResult)
            .Add(contentResult)
            .Build(() =>
                (
                    titleResult.Value,
                    contentResult.Value
                )
            );
    }
}
