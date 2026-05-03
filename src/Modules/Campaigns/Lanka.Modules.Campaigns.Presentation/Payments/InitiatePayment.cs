using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Payments.Initiate;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Payments;

internal sealed class InitiatePayment : PaymentsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.CreatePayment];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BuildRoute("{campaignId:guid}/payment/initiate"),
                async (Guid campaignId, ISender sender, CancellationToken cancellationToken) =>
                {
                    Result<LiqPayCheckoutResponse> result =
                        await sender.Send(new InitiatePaymentCommand(campaignId), cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Payments)
            .WithName("InitiatePayment")
            .WithSummary("Initiate payment")
            .WithDescription("Creates a pending payment and returns LiqPay checkout params");
    }
}
