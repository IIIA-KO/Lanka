using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Reviews.GetBloggerReviews;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Reviews;

internal sealed class GetBloggerReviews : ReviewEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadReviews];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("{bloggerId:guid}"),
            async (
                [FromRoute] Guid bloggerId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                Result<IReadOnlyList<ReviewResponse>> result = await sender.Send(
                    new GetBloggerReviewsQuery(bloggerId),
                    cancellationToken
                );

                return result.Match(Results.Ok, ApiResult.Problem);
            }
        )
        .WithTags(Tags.Reviews)
        .WithName("GetBloggerReviews")
        .WithSummary("Get blogger reviews")
        .WithDescription("Retrieves all reviews for a specific blogger")
        .WithOpenApi();
    }
}
