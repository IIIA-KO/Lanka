using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Reviews.Delete;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Reviews;

internal sealed class DeleteReview : ReviewEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.DeleteReview];
    
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapDelete(this.BuildRoute("{id:guid}"),
                async (
                    [FromRoute] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(
                        new DeleteReviewCommand(id),
                        cancellationToken
                    );

                    return result.Match(Results.NoContent, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Reviews)
            .WithName("DeleteReview")
            .WithSummary("Delete review")
            .WithDescription("Deletes an existing review")
            .WithOpenApi();
    }
}
