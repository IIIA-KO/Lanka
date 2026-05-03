using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Payments.GetPayment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Payments;

internal sealed class GetPayment : PaymentsEndpointBase
{
    protected override string[] RequiredPermissions => [Permissions.ReadPayments];

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("{campaignId:guid}/payment"),
                async (Guid campaignId, ISender sender, CancellationToken cancellationToken) =>
                {
                    Result<PaymentResponse> result =
                        await sender.Send(new GetPaymentQuery(campaignId), cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .WithTags(Tags.Payments)
            .WithName("GetPayment")
            .WithSummary("Get payment")
            .WithDescription("Returns payment status for a campaign");
    }
}
