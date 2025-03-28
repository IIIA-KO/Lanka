using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Offers.GetBloggerAverageOfferPrices;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Offers;

internal sealed class GetBloggerAverageOfferPrices : OfferEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("average-prices/{bloggerId:guid}"),
                async (
                    [FromRoute] Guid bloggerId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<List<AveragePriceResponse>> result = await sender.Send(
                        new GetBloggerAverageOfferPricesQuery(bloggerId),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Offers);
    }
}
