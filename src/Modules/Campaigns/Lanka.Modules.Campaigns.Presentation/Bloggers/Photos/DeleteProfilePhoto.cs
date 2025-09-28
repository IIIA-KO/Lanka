using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Bloggers.Photos.DeleteProfilePhoto;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers.Photos;

internal sealed class DeleteProfilePhoto : BloggerEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapDelete(this.BuildRoute("photos"),
            async (
                ISender sender,
                CancellationToken cancellationToken
            ) =>
            {
                Result result = await sender.Send(
                    new DeleteProfilePhotoCommand(),
                    cancellationToken
                );

                return result.Match(Results.NoContent, ApiResult.Problem);
            }
        )
        .WithTags(Tags.Bloggers)
        .WithName("DeleteProfilePhoto")
        .WithSummary("Delete profile photo")
        .WithDescription("Deletes the current blogger's profile photo")
        .WithOpenApi();
    }
}
