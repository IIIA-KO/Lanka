using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Matching.Application.SearchableDocuments.SearchSimilar;
using Lanka.Modules.Matching.Domain.SearchableItems;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Matching.Presentation.SearchableDocuments;

internal sealed class SearchSimilar : SearchEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("similar/{sourceItemId:guid}"),
                async (Guid sourceItemId,
                    SearchableItemType sourceType,
                    string? itemTypes,
                    DateTimeOffset? createdAfter,
                    DateTimeOffset? createdBefore,
                    bool? onlyActive,
                    int? page,
                    int? size,
                    ISender sender
                ) =>
                {
                    var query = new SearchSimilarQuery(
                        sourceItemId,
                        sourceType,
                        itemTypes,
                        NumericFilters: null,
                        FacetFilters: null,
                        createdAfter,
                        createdBefore,
                        onlyActive ?? true,
                        page ?? 1,
                        size ?? 20
                    );

                    Result<SearchSimilarResponse> result = await sender.Send(query);

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Search)
            .WithName("SearchSimilar")
            .WithSummary("Find similar documents")
            .WithDescription("Finds documents similar to the specified source document")
            .WithOpenApi();
    }
}
