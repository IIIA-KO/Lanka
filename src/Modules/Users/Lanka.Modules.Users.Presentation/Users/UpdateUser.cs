using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Application.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

public class UpdateUser : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("{id:guid}"),
            async (
                [FromRoute] Guid id,
                [FromBody] UpdateUserRequest request,
                ISender sender,
                CancellationToken cancellationToken
            ) =>
            {
                Result<UserResponse> result = await sender.Send(
                    new UpdateUserCommand(id, request.FirstName, request.LastName, request.DateOfBirth),
                    cancellationToken
                );
                
                return result.Match(Results.Ok, ApiResult.Problem);
            }
        );
    }
    
    internal sealed class UpdateUserRequest
    {
        public string FirstName { get; init; }

        public string LastName { get; init; }

        public DateOnly DateOfBirth { get; init; }
    }
}
