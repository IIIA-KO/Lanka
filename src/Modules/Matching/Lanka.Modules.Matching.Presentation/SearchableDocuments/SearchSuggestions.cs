using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Matching.Application.SearchableDocuments.Suggestions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Matching.Presentation.SearchableDocuments;

internal sealed class SearchSuggestions : SearchEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("suggestions"),
                async (
                    string q,
                    int? itemType,
                    int? limit,
                    ISender sender
                ) =>
                {
                    var query = new GetSearchSuggestionsQuery(q, itemType, limit ?? 10);

                    Result<IReadOnlyCollection<string>> result = await sender.Send(query);

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Search)
            .WithName("SearchSuggestions")
            .WithSummary("Get search suggestions")
            .WithDescription("Returns autocomplete suggestions based on partial search query");
    }
}
