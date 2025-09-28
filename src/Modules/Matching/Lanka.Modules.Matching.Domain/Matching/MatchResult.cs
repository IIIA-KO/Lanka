using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Domain.Matching;

public sealed class MatchResult
{
    public Guid ItemId { get; }
    public SearchableItemType Type { get; }
    public double RelevanceScore { get; }
    public string HighlightedSnippet { get; }

    public MatchResult(
        Guid itemId,
        SearchableItemType type,
        double relevanceScore, 
        string highlightedSnippet
    )
    {
        this.ItemId = itemId;
        this.Type = type;
        this.RelevanceScore = relevanceScore;
        this.HighlightedSnippet = highlightedSnippet;
    }
}
