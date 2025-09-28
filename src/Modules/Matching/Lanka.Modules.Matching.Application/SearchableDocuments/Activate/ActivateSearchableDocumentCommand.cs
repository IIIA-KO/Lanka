using Lanka.Common.Application.Messaging;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Activate;

public sealed record ActivateSearchableDocumentCommand(
    Guid SourceEntityId,
    SearchableItemType Type
) : ICommand;
