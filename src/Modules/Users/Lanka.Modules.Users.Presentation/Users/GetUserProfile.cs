using System.Security.Claims;
using Lanka.Common.Domain;
using Lanka.Common.Infrastructure.Authentication;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.GetUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

internal sealed class GetUserProfile : UsersEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadProfile];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("profile"),
                async (ClaimsPrincipal claims, ISender sender) =>
                {
                    Result<UserResponse> result = await sender.Send(new GetUserQuery(claims.GetUserId()));
                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Users)
            .WithName("GetUserProfile")
            .WithSummary("Get user profile")
            .WithDescription("Retrieves the current user's profile information");
    }
}
