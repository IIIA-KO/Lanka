using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.RegisterUser;
using Lanka.Modules.Users.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users;

public class RegisterUser : UsersEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("register"),
                async (Request request, ISender sender) =>
                {
                    Result<UserId> result = await sender.Send(new RegisterUserCommand(
                        request.Email,
                        request.Password,
                        request.FirstName,
                        request.LastName,
                        request.BirthDate
                    ));

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .AllowAnonymous()
            .WithTags(Tags.Users);
    }
        
    internal sealed class Request
    {
        public string Email { get; init; }

        public string Password { get; init; }

        public string FirstName { get; init; }

        public string LastName { get; init; }
            
        public DateOnly BirthDate { get; init; }
    }    
}