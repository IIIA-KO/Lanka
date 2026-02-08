using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Reviews.Edit;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Reviews;

internal sealed class EditReview : ReviewEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.UpdateReview];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute("{id:guid}"),
                async (
                    [FromRoute] Guid id,
                    [FromBody] EditReviewRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<ReviewResponse> result = await sender.Send(new EditReviewCommand(
                            id,
                            request.Rating,
                            request.Content),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Reviews)
            .WithName("EditReview")
            .WithSummary("Edit review")
            .WithDescription("Updates an existing review");
    }

    internal sealed class EditReviewRequest
    {
        public string Content { get; init; }

        public int Rating { get; init; }
    }
}
