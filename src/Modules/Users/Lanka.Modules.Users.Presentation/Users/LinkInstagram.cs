using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Instagram.Link;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

internal sealed class LinkInstagram : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("link-instagram"),
                async (
                    [FromBody] LinkInstagramRequest request,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result result = await sender.Send(
                        new LinkInstagramAccountCommand(request.Code),
                        cancellationToken
                    );

                    return result.Match(() => Results.Accepted(), ApiResult.Problem);
                }
            )
            .WithTags(Tags.Users);
    }

    internal sealed class LinkInstagramRequest
    {
        public string Code { get; init; }
    }
}
