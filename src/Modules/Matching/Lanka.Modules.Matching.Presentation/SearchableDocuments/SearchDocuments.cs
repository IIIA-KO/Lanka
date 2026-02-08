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
                    string q,
                    bool? fuzzy,
                    bool? synonyms,
                    double? fuzzyDistance,
                    string? itemTypes,
                    DateTimeOffset? createdAfter,
                    DateTimeOffset? createdBefore,
                    bool? onlyActive,
                    int? page,
                    int? size,
                    ISender sender
                ) =>
                {
                    var query = new SearchDocumentsQuery(
                        q,
                        fuzzy ?? true,
                        synonyms ?? true,
                        fuzzyDistance ?? 0.8,
                        itemTypes,
                        NumericFilters: null,
                        FacetFilters: null,
                        createdAfter,
                        createdBefore,
                        onlyActive ?? true,
                        page ?? 1,
                        size ?? 20
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
