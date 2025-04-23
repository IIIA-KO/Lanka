using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.Login;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

internal sealed class Login : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("login"),
                async ( 
                    [FromBody] LoginUserRequest request,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<AccessTokenResponse> result = await sender.Send(
                        new LoginUserCommand(request.Email, request.Password),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .AllowAnonymous()
            .WithTags(Tags.Users);
    }

    internal sealed class LoginUserRequest
    {
        public string Email { get; init; }

        public string Password { get; init; }
    }
}
