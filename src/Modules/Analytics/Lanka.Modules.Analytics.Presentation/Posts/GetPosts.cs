using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Application.Instagram.GetPosts;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Posts;

internal sealed class GetPosts : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("posts"),
                async (
                    [FromQuery] int limit,
                    [FromQuery] string? cursorType,
                    [FromQuery] string? cursor,
                    IUserContext userContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<PostsResponse> result = await sender.Send(
                        new GetPostsQuery(userContext.GetUserId(), limit, cursorType, cursor),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetPosts")
            .WithSummary("Get Instagram posts")
            .WithDescription("Retrieves Instagram posts with pagination support using cursor-based navigation");
    }
}
