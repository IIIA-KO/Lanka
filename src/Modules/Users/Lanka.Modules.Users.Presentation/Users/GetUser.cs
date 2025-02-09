using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Users.Application.Users.GetUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Users.Presentation.Users
{
    internal sealed class GetUser : UsersEndpointBase
    {
        protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
        {
            return app.MapGet(this.BuildRoute("{id}"),
                    async (Guid id, ISender sender) =>
                    {
                        Result<UserResponse> result = await sender.Send(new GetUserQuery(id));
                        return result.Match(Results.Ok, ApiResult.Problem);
                    })
                .WithTags(Tags.Users);
        }
    }
}
