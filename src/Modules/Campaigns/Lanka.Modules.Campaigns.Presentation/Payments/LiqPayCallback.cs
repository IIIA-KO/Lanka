using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Campaigns.Application.Payments.ProcessCallback;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Payments;

internal sealed class LiqPayCallback : EndpointBase
{
    protected override string BaseRoute => "payments/liqpay/callback";

    protected override bool RequireAuthorization => false;

    protected override string[]? RequiredPermissions => null;

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPost(this.BaseRoute,
                async (
                    [FromForm] string data,
                    [FromForm] string signature,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result result = await sender.Send(
                        new ProcessLiqPayCallbackCommand(data, signature),
                        cancellationToken
                    );

                    return result.Match(() => Results.Ok(), ApiResult.Problem);
                })
            .DisableAntiforgery()
            .WithTags(Tags.Payments)
            .WithName("LiqPayCallback")
            .WithSummary("LiqPay callback")
            .WithDescription("Receives payment status from LiqPay (public endpoint, no auth)");
    }
}
