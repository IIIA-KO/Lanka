using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Reviews.Create;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Reviews;

internal sealed class CreateReview : ReviewEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.CreateReview];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute(string.Empty),
                async (
                    [FromBody] CreateReviewRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<Guid> result = await sender.Send(new CreateReviewCommand(
                            request.CampaignId,
                            request.Rating,
                            request.Comment
                        ),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Reviews);
    }

    internal sealed class CreateReviewRequest
    {
        public Guid CampaignId { get; init; }

        public int Rating { get; init; }

        public string Comment { get; init; }
    }
}
