using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Offers.Edit;
using Lanka.Modules.Campaigns.Application.Offers.GetOffer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Offers;

internal sealed class EditOffer : OfferEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(this.BuildRoute("{id:guid}"),
            async (
                [FromRoute] Guid id,
                [FromBody] EditOfferRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                Result<OfferResponse> result = await sender.Send(new EditOfferCommand(
                        id,
                        request.Name,
                        request.PriceAmount,
                        request.PriceCurrency,
                        request.Description),
                    cancellationToken
                );

                return result.Match(Results.Ok, ApiResult.Problem);
            }
        )
        .WithTags(Tags.Offers);
    }

    internal sealed class EditOfferRequest
    {
        public string Name { get; init; }

        public decimal PriceAmount { get; init; }

        public string PriceCurrency { get; init; }

        public string Description { get; init; }
    }
}
