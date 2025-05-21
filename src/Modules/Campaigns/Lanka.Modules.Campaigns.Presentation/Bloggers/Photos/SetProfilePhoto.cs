using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Bloggers.Photos.SetProfilePhoto;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers.Photos;

internal sealed class SetProfilePhoto : BloggerEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("photos"),
            async (
                [FromForm] IFormFile photo,
                ISender sender,
                CancellationToken cancellationToken
            ) =>
            {
                Result result = await sender.Send(
                    new SetProfilePhotoCommand(photo),
                    cancellationToken
                );
                
                return result.Match(Results.NoContent, ApiResult.Problem);
            }
        );
    }
}
