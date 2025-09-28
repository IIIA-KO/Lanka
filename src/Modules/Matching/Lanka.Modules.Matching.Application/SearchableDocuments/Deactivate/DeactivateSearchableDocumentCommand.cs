using Lanka.Common.Application.Messaging;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Deactivate;

public sealed record DeactivateSearchableDocumentCommand(
    Guid SourceEntityId,
    SearchableItemType Type
) : ICommand;
