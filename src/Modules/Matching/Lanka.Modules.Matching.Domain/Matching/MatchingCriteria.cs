namespace Lanka.Modules.Matching.Domain.Matching;

public sealed class MatchingCriteria
{
    public string Query { get; }
    public double? MinimumRelevance { get; }
    public IReadOnlyCollection<string> Facets { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public MatchingCriteria(
        string query,
        double? minimumRelevance = null,
        IEnumerable<string>? facets = null,
        int pageNumber = 1,
        int pageSize = 20
    )
    {
        this.Query = query?.Trim() ?? string.Empty;
        this.MinimumRelevance = minimumRelevance;
        this.Facets = facets?.ToList() ?? [];
        this.PageNumber = pageNumber > 0 ? pageNumber : 1;
        this.PageSize = pageSize > 0 ? pageSize : 20;
    }
}
