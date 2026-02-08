using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.Delete;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

internal sealed class DeleteUser : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapDelete(this.BuildRoute(string.Empty),
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                Result result = await sender.Send(new DeleteUserCommand(), cancellationToken);

                return result.Match(Results.NoContent, ApiResult.Problem);
            }
        )
        .WithTags(Tags.Users)
        .WithName("DeleteUser")
        .WithSummary("Delete user account")
        .WithDescription("Permanently deletes the current user's account and all associated data");
    }
}
