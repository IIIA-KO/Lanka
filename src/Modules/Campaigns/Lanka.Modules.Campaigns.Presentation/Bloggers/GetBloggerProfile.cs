using System.Security.Claims;
using Lanka.Common.Domain;
using Lanka.Common.Infrastructure.Authentication;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers;

internal sealed class GetBloggerProfile : BloggerEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("profile"),
                async (ClaimsPrincipal claims, ISender sender) =>
                {
                    Result<BloggerResponse> result = await sender.Send(
                        new GetBloggerQuery(claims.GetUserId())
                    );
                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Bloggers)
            .WithName("GetBloggerProfile")
            .WithSummary("Get blogger profile")
            .WithDescription("Retrieves the current blogger's profile information");
    }
}
