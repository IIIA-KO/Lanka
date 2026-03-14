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

internal sealed class GetBloggerPosts : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("posts/{userId:guid}"),
                async (
                    [FromRoute] Guid userId,
                    [FromQuery] int limit,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<PostsResponse> result = await sender.Send(
                        new GetPostsQuery(userId, limit, null, null),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetBloggerPosts")
            .WithSummary("Get blogger Instagram posts")
            .WithDescription("Retrieves Instagram posts for a specific blogger");
    }
}
