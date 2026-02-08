using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

public class RegisterUser : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("register"),
                async (
                    [FromBody] RegisterUserRequest request,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<Guid> result = await sender.Send(new RegisterUserCommand(
                        request.Email,
                        request.Password,
                        request.FirstName,
                        request.LastName,
                        request.BirthDate
                    ), cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .AllowAnonymous()
            .WithTags(Tags.Users)
            .WithName("RegisterUser")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with email, password, and basic profile information");
    }

    internal sealed class RegisterUserRequest
    {
        public string Email { get; init; }

        public string Password { get; init; }

        public string FirstName { get; init; }

        public string LastName { get; init; }

        public DateOnly BirthDate { get; init; }
    }
}
