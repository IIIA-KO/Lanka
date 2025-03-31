using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Pacts;

internal sealed class GetBloggerPact : PactEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(string.Empty,
                async (
                    [FromQuery] Guid bloggerId,
                    ISender sender,
                    CancellationToken cancellation) =>
                {
                    Result<PactResponse> result = await sender.Send(
                        new GetBloggerPactQuery(bloggerId),
                        cancellation
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Pacts);
    }
}
