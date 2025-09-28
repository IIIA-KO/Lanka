using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Offers.Create;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Offers;

internal sealed class CreateOffer : OfferEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.CreateOffer];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute(string.Empty),
                async (
                    [FromBody] CreateOfferRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    Result<Guid> result = await sender.Send(new CreateOfferCommand(
                            request.Name,
                            request.PriceAmount,
                            request.PriceCurrency,
                            request.Description
                        ),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Offers)
            .WithName("CreateOffer")
            .WithSummary("Create offer")
            .WithDescription("Creates a new offer for campaigns")
            .WithOpenApi();
    }

    internal sealed class CreateOfferRequest
    {
        public string Name { get; init; }
        public decimal PriceAmount { get; init; }
        public string PriceCurrency { get; init; }
        public string Description { get; init; }
    }
}
