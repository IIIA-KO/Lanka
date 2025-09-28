using Lanka.Common.Application.Messaging;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Index;

public sealed record IndexDocumentCommand(
    Guid SourceEntityId,
    SearchableItemType Type,
    string Title,
    string Content,
    IReadOnlyCollection<string> Tags,
    IDictionary<string, object>? Metadata = null
) : ICommand;


