using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Offers.GetOffer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Offers;

internal sealed class GetOffer : OfferEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet("{id:guid}",
            async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                Result<OfferResponse> result = await sender.Send(
                    new GetOfferQuery(id),
                    cancellationToken
                );

                return result.Match(Results.Ok, ApiResult.Problem);
            }
        )
        .WithTags(Tags.Offers);
    }
}
