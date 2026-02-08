using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Instagram.GetLinkingStatus;
using Lanka.Modules.Users.Application.Instagram.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

internal sealed class GetInstagramLinkingStatus : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("link-instagram/status"),
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    Result<InstagramOperationStatus> result = await sender.Send(
                        new GetInstagramLinkingStatusQuery(),
                        cancellationToken
                    );

                    return result.Match(
                        status => Results.Ok(new { status = status.Status, message = status.Message, timestamp = status.StartedAt }),
                        ApiResult.Problem
                    );
                })
            .WithTags(Tags.Users)
            .WithName("GetInstagramLinkingStatus")
            .WithSummary("Get Instagram linking status")
            .WithDescription("Retrieves the current status of Instagram account linking operation")
            .RequireAuthorization();
    }
}
