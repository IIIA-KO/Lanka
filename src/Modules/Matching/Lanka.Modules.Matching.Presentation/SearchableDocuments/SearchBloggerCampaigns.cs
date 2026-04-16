using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Matching.Application.SearchableDocuments.Search;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Matching.Presentation.SearchableDocuments;

internal sealed class SearchBloggerCampaigns : SearchEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("blogger-campaigns"),
                async (
                    Guid bloggerId,
                    string? q,
                    string? status,
                    DateTimeOffset? scheduledAfter,
                    DateTimeOffset? scheduledBefore,
                    int? page,
                    int? size,
                    ISender sender
                ) =>
                {
                    // Only show campaigns that haven't been hard-rejected (they're still isActive=true)
                    // "OR" filter: match where CreatorId = bloggerId OR ClientId = bloggerId
                    var orFacetFilters = new List<(string Field, IReadOnlyCollection<string> Values)>
                    {
                        ("CreatorId", [bloggerId.ToString()]),
                        ("ClientId",  [bloggerId.ToString()])
                    };

                    Dictionary<string, IReadOnlyCollection<string>>? facetFilters = null;
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        facetFilters = new Dictionary<string, IReadOnlyCollection<string>>
                        {
                            ["Status"] = status.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        };
                    }

                    var query = new SearchDocumentsQuery(
                        q ?? string.Empty,
                        EnableFuzzySearch: false,
                        EnableSynonyms: false,
                        FuzzyDistance: 0.8,
                        ItemTypes: "3",   // SearchableItemType.Campaign
                        FacetFilters: facetFilters,
                        OrFacetFilters: orFacetFilters,
                        CreatedAfter: scheduledAfter,
                        CreatedBefore: scheduledBefore,
                        OnlyActive: false,  // Show all statuses including cancelled/rejected history
                        Page: page ?? 1,
                        Size: size ?? 20
                    );

                    Result<SearchDocumentsResponse> result = await sender.Send(query);

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Search)
            .WithName("SearchBloggerCampaigns")
            .WithSummary("Search campaigns for a specific blogger (as creator or client) with pagination")
            .WithDescription("Returns paginated campaigns for the given blogger using Elasticsearch");
    }
}
