using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Application.Users.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

internal sealed class RefreshToken : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("refresh-token"),
            async (
                [FromBody] RefreshTokenRequest request,
                ISender sender,
                CancellationToken cancellationToken
            ) =>
            {
                Result<AccessTokenResponse> result = await sender.Send(
                    new RefreshTokenCommand(request.RefreshToken),
                    cancellationToken
                );
                
                return result.Match(Results.Ok, ApiResult.Problem);
            }
        )
        .AllowAnonymous()
        .WithTags(Tags.Users)
        .WithName("RefreshToken")
        .WithSummary("Refresh access token")
        .WithDescription("Refreshes an expired access token using a valid refresh token")
        .WithOpenApi();
    }
    
    internal sealed class RefreshTokenRequest
    {
        public string RefreshToken { get; init; }
    }
}
