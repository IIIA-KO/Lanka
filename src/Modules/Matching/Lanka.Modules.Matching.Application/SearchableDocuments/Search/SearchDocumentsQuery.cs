using System.Globalization;
using System.Text;
using System.Text.Json;
using Lanka.Common.Application.Caching;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.Search;

public sealed record SearchDocumentsQuery(
    string Query,
    bool EnableFuzzySearch = true,
    bool EnableSynonyms = true,
    double FuzzyDistance = 0.8,
    string? ItemTypes = null,
    IDictionary<string, object>? NumericFilters = null,
    IDictionary<string, IReadOnlyCollection<string>>? FacetFilters = null,
    DateTimeOffset? CreatedAfter = null,
    DateTimeOffset? CreatedBefore = null,
    bool OnlyActive = true,
    int Page = 1,
    int Size = 20
) : ICachedQuery<SearchDocumentsResponse>
{
    public string CacheKey => this.GenerateCacheKey();
    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);

    private string GenerateCacheKey()
    {
        var keyBuilder = new StringBuilder();

        string queryHash = string.IsNullOrEmpty(this.Query)
            ? "empty"
            : this.Query.GetHashCode().ToString(CultureInfo.InvariantCulture);

        keyBuilder.Append(CultureInfo.InvariantCulture, $"search-docs-{queryHash}");

        keyBuilder.Append(CultureInfo.InvariantCulture,
            $"-fuzzy-{this.EnableFuzzySearch}-syn-{this.EnableSynonyms}-dist-{this.FuzzyDistance:F1}");

        if (!string.IsNullOrEmpty(this.ItemTypes))
        {
            keyBuilder.Append(CultureInfo.InvariantCulture, $"-types-{this.ItemTypes}");
        }

        if (this.NumericFilters?.Any() == true)
        {
            int numericHash = JsonSerializer.Serialize(this.NumericFilters).GetHashCode();
            keyBuilder.Append(CultureInfo.InvariantCulture, $"-numeric-{numericHash}");
        }

        if (this.FacetFilters?.Any() == true)
        {
            int facetHash = JsonSerializer.Serialize(this.FacetFilters).GetHashCode();
            keyBuilder.Append(CultureInfo.InvariantCulture, $"-facets-{facetHash}");
        }

        if (this.CreatedAfter.HasValue)
        {
            keyBuilder.Append(CultureInfo.InvariantCulture, $"-after-{this.CreatedAfter.Value:yyyy-MM-dd}");
        }

        if (this.CreatedBefore.HasValue)
        {
            keyBuilder.Append(CultureInfo.InvariantCulture, $"-before-{this.CreatedBefore.Value:yyyy-MM-dd}");
        }

        keyBuilder.Append(CultureInfo.InvariantCulture, $"-active-{this.OnlyActive}-page-{this.Page}-size-{this.Size}");

        return keyBuilder.ToString();
    }
}
