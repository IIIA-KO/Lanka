using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Campaigns.Application.Bloggers.Update;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers;

internal sealed class UpdateBlogger : BloggerEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute(string.Empty),
            async (
                [FromBody] UpdateBloggerRequest request,
                ISender sender,
                CancellationToken cancellation
            ) =>
            {
                Result<BloggerResponse> result = await sender.Send(
                    new UpdateBloggerCommand(
                        request.FirstName,
                        request.LastName,
                        request.BirthDate,
                        request.Bio
                    ),
                    cancellation
                );

                return result.Match(Results.Ok, ApiResult.Problem);
            }
        )
        .WithTags(Tags.Bloggers)
        .WithName("UpdateBlogger")
        .WithSummary("Update blogger profile")
        .WithDescription("Updates the current blogger's profile information")
        .WithOpenApi();
    }

    internal sealed class UpdateBloggerRequest
    {
        public string FirstName { get; init; }

        public string LastName { get; init; }

        public DateOnly BirthDate { get; init; }

        public string Bio { get; init; }
    }
}
