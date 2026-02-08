using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Pacts.Edit;
using Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Pacts;

internal sealed class EditPact : PactEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.UpdatePact];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute("{id:guid}/edit"),
                async (
                    [FromBody] EditPactRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<PactResponse> result = await sender.Send(
                        new EditPactCommand(request.Content),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Pacts)
            .WithName("EditPact")
            .WithSummary("Edit pact")
            .WithDescription("Updates an existing pact's content");
    }

    internal sealed class EditPactRequest
    {
        public string Content { get; init; }
    }
}
