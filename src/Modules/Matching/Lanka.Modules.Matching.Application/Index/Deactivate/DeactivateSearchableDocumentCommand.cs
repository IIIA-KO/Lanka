using Lanka.Common.Application.Messaging;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.Index.Deactivate;

public sealed record DeactivateSearchableDocumentCommand(
    Guid SourceEntityId,
    SearchableItemType Type
) : ICommand;
