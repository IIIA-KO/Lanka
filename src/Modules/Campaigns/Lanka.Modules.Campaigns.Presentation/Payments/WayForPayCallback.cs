using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Campaigns.Application.Abstractions.Payments;
using Lanka.Modules.Campaigns.Application.Payments.ProcessCallback;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Payments;

internal sealed class WayForPayCallback : EndpointBase
{
    protected override string BaseRoute => "payments/wayforpay/callback";

    protected override bool RequireAuthorization => false;

    protected override string[]? RequiredPermissions => null;

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BaseRoute,
                async (HttpRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    using var reader = new StreamReader(request.Body);
                    string payload = await reader.ReadToEndAsync(cancellationToken);

                    Result<PaymentCallbackResponse> result = await sender.Send(
                        new ProcessPaymentCallbackCommand(payload),
                        cancellationToken);

                    return result.Match(Results.Ok, ApiResult.Problem);
                })
            .DisableAntiforgery()
            .WithTags(Tags.Payments)
            .WithName("WayForPayCallback")
            .WithSummary("WayForPay callback")
            .WithDescription("Receives payment status from WayForPay (public endpoint, no auth)");
    }
}
