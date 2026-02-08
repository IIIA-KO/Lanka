using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers;

internal sealed class GetBlogger : BloggerEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("{id:guid}"),
                async (Guid id, ISender sender) =>
                {
                    Result<BloggerResponse> result = await sender.Send(
                        new GetBloggerQuery(id)
                    );
                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Bloggers)
            .WithName("GetBlogger")
            .WithSummary("Get blogger")
            .WithDescription("Retrieves blogger information by ID");
    }
}
