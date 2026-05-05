using Lanka.Common.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Lanka.Modules.Campaigns.Presentation.Payments;

internal sealed class WayForPayReturn : EndpointBase
{
    protected override string BaseRoute => "payments/wayforpay/return";

    protected override bool RequireAuthorization => false;

    protected override string[]? RequiredPermissions => null;

    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        RouteHandlerBuilder post = app.MapPost(
                this.BaseRoute,
                (IConfiguration configuration) => RedirectToClient(configuration))
            .DisableAntiforgery();

        app.MapGet(
            this.BaseRoute,
            (IConfiguration configuration) => RedirectToClient(configuration));

        return post
            .WithTags(Tags.Payments)
            .WithName("WayForPayReturn")
            .WithSummary("WayForPay browser return")
            .WithDescription("Accepts WayForPay browser return and redirects to the client application");
    }

    private static IResult RedirectToClient(IConfiguration configuration)
    {
        string? url = configuration["Campaigns:WayForPay:ClientReturnUrl"];
        return Results.Redirect(string.IsNullOrWhiteSpace(url) ? "/campaigns" : url);
    }
}
