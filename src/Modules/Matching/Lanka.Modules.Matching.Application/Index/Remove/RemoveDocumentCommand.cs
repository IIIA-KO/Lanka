using Lanka.Common.Application.Messaging;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.Index.Remove;

public sealed record RemoveDocumentCommand(
    Guid SourceEntityId,
    SearchableItemType Type
) : ICommand;
