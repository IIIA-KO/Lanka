using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Suggestions;

public sealed record GetSearchSuggestionsQuery(
    string Query,
    int? ItemType = null,
    int Limit = 10
) : IQuery<IReadOnlyCollection<string>>;
