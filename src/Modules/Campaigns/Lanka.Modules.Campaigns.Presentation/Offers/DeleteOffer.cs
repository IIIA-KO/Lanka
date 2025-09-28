using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Offers.Delete;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Offers;

internal sealed class DeleteOffer : OfferEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.DeleteOffer];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapDelete(this.BuildRoute("{id:guid}"),
                async (
                    [FromRoute] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await sender.Send(
                        new DeleteOfferCommand(id),
                        cancellationToken
                    );

                    return result.Match(Results.NoContent, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Offers)
            .WithName("DeleteOffer")
            .WithSummary("Delete offer")
            .WithDescription("Deletes an existing offer")
            .WithOpenApi();
    }
}
