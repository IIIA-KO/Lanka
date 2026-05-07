using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Bloggers.GetPayoutAccount;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers;

internal sealed class GetPayoutAccount : BloggerEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(
            this.BuildRoute("me/payout-account"),
            async (ISender sender, CancellationToken cancellation) =>
            {
                Result<PayoutAccountResponse> result =
                    await sender.Send(new GetPayoutAccountQuery(), cancellation);

                return result.Match(Results.Ok, ApiResult.Problem);
            })
            .RequireAuthorization()
            .WithTags(Tags.Bloggers)
            .WithName("GetPayoutAccount")
            .WithSummary("Get payout account")
            .WithDescription("Returns the current blogger's payout account (IBAN masked)");
    }
}
