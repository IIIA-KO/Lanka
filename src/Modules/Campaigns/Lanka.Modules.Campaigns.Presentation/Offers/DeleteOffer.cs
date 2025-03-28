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
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapDelete("{id:guid}",
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
        .WithTags(Tags.Offers);
    }
}
