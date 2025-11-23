using Lanka.Common.Application.Messaging;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.Index.Update;

public sealed record UpdateSearchableDocumentContentCommand(
    Guid SourceEntityId,
    SearchableItemType Type,
    string Title,
    string Content,
    IEnumerable<string> Tags,
    IDictionary<string, object>? Metadata
) : ICommand;
