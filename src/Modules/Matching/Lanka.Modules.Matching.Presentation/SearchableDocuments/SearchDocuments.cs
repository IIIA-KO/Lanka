using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Matching.Application.SearchableDocuments.Search;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Matching.Presentation.SearchableDocuments;

internal sealed class SearchDocuments : SearchEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute(string.Empty),
                async (
                    string? q,
                    bool? fuzzy,
                    bool? synonyms,
                    double? fuzzyDistance,
                    string? itemTypes,
                    double? priceMin,
                    double? priceMax,
                    double? followersMin,
                    double? followersMax,
                    double? engagementRateMin,
                    double? engagementRateMax,
                    string? category,
                    string? audienceCountry,
                    string? audienceGender,
                    string? audienceAgeGroup,
                    DateTimeOffset? createdAfter,
                    DateTimeOffset? createdBefore,
                    bool? onlyActive,
                    int? page,
                    int? size,
                    Guid? excludeItemId,
                    ISender sender
                ) =>
                {
                    Dictionary<string, object> numericFilters = [];

                    if (priceMin.HasValue)
                    {
                        numericFilters["PriceAmountMin"] = priceMin.Value;
                    }

                    if (priceMax.HasValue)
                    {
                        numericFilters["PriceAmountMax"] = priceMax.Value;
                    }

                    if (followersMin.HasValue)
                    {
                        numericFilters["FollowersCountMin"] = followersMin.Value;
                    }

                    if (followersMax.HasValue)
                    {
                        numericFilters["FollowersCountMax"] = followersMax.Value;
                    }

                    if (engagementRateMin.HasValue)
                    {
                        numericFilters["EngagementRateMin"] = engagementRateMin.Value;
                    }

                    if (engagementRateMax.HasValue)
                    {
                        numericFilters["EngagementRateMax"] = engagementRateMax.Value;
                    }

                    Dictionary<string, IReadOnlyCollection<string>> facetFilters = [];

                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        facetFilters["Category"] = [category];
                    }

                    if (!string.IsNullOrWhiteSpace(audienceCountry))
                    {
                        facetFilters["AudienceTopCountry"] = [audienceCountry];
                    }

                    if (!string.IsNullOrWhiteSpace(audienceGender))
                    {
                        facetFilters["AudienceTopGender"] = [audienceGender];
                    }

                    if (!string.IsNullOrWhiteSpace(audienceAgeGroup))
                    {
                        facetFilters["AudienceTopAgeGroup"] = [audienceAgeGroup];
                    }

                    var query = new SearchDocumentsQuery(
                        q ?? string.Empty,
                        fuzzy ?? true,
                        synonyms ?? true,
                        fuzzyDistance ?? 0.8,
                        itemTypes,
                        numericFilters.Count > 0 ? numericFilters : null,
                        facetFilters.Count > 0 ? facetFilters : null,
                        null,
                        createdAfter,
                        createdBefore,
                        onlyActive ?? true,
                        page ?? 1,
                        size ?? 20,
                        excludeItemId
                    );

                    Result<SearchDocumentsResponse> result = await sender.Send(query);

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Search)
            .WithName("SearchDocuments")
            .WithSummary("Search documents")
            .WithDescription("Performs full-text search across all searchable documents with filters and pagination");
    }
}
