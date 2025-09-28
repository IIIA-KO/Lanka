using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Instagram.RenewAccess;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

internal sealed class RenewInstagramAccess : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("renew-instagram-access"),
                async (
                    [FromBody] RenewInstagramAccessRequest request,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result result = await sender.Send(
                        new RenewInstagramAccessCommand(request.Code),
                        cancellationToken
                    );

                    return result.Match(() => Results.Accepted(), ApiResult.Problem);
                })
            .WithTags(Tags.Users)
            .WithName("RenewInstagramAccess")
            .WithSummary("Renew Instagram access token")
            .WithDescription("Renews the Instagram access token using a fresh authorization code")
            .WithOpenApi();
    }

    internal sealed class RenewInstagramAccessRequest
    {
        public string Code { get; init; }
    }
}
