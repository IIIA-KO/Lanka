using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Pacts.Create;
using Lanka.Modules.Campaigns.Domain.Pacts;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Pacts;

internal sealed class CreatePact : PactEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.CreatePact];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute(string.Empty),
                async (
                    [FromBody] CreatePactRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<PactId> result = await sender.Send(
                        new CreatePactCommand(request.Content),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Pacts);
    }

    internal sealed class CreatePactRequest
    {
        public string Content { get; init; }
    }
}
