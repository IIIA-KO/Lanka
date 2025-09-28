using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.Searches.SearchResults.SearchHighlights;

public sealed record SearchHighlight
{
    public string FieldName { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Fragments { get; init; } = [];

    private SearchHighlight() { }

    public static Result<SearchHighlight> Create(string fieldName, IReadOnlyCollection<string>? fragments)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return Result.Failure<SearchHighlight>(SearchHighlightErrors.EmptyFieldName);
        }

        if (fragments is null || !fragments.Any())
        {
            return Result.Failure<SearchHighlight>(SearchHighlightErrors.EmptyFragments);
        }

        return new SearchHighlight
        {
            FieldName = fieldName,
            Fragments = fragments
        };
    }
}
