using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Campaigns.Application.Bloggers.UpdatePayoutAccount;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers;

internal sealed class UpdatePayoutAccount : BloggerEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapPut(
            this.BuildRoute("me/payout-account"),
            async (
                [FromBody] UpdatePayoutAccountRequest request,
                ISender sender,
                CancellationToken cancellation) =>
            {
                Result result = await sender.Send(
                    new UpdatePayoutAccountCommand(request.Iban, request.Currency),
                    cancellation
                );

                return result.Match(Results.NoContent, ApiResult.Problem);
            })
            .RequireAuthorization()
            .WithTags(Tags.Bloggers)
            .WithName("UpdatePayoutAccount")
            .WithSummary("Update payout account")
            .WithDescription("Sets or updates the current blogger's IBAN and payout currency");
    }

    internal sealed class UpdatePayoutAccountRequest
    {
        public string Iban { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
    }
}
