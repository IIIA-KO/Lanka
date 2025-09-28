using System.Globalization;
using System.Text;
using System.Text.Json;
using Lanka.Common.Application.Caching;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.SearchableDocuments.SearchSimilar;

public sealed record SearchSimilarQuery(
    Guid SourceItemId,
    SearchableItemType SourceType,
    string? ItemTypes = null,
    IDictionary<string, object>? NumericFilters = null,
    IDictionary<string, IReadOnlyCollection<string>>? FacetFilters = null,
    DateTimeOffset? CreatedAfter = null,
    DateTimeOffset? CreatedBefore = null,
    bool OnlyActive = true,
    int Page = 1,
    int Size = 20
) : ICachedQuery<SearchSimilarResponse>
{
    public string CacheKey => this.GenerateCacheKey();
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

    private string GenerateCacheKey()
    {
        var keyBuilder = new StringBuilder();
        keyBuilder.Append(CultureInfo.InvariantCulture, $"search-similar-{this.SourceItemId}-{(int)this.SourceType}");

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
