using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.ProfileSummary;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Presentation.Internal;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.ProfileSummary;

internal sealed class GetProfileSummary : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("profile-summary"),
                async (
                    [FromQuery] StatisticsPeriod period,
                    [FromServices] ISender sender,
                    [FromServices] IUserContext userContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<ProfileSummaryResponse> result =
                        await sender.Send(
                            new GetProfileSummaryQuery(userContext.GetUserId(), period),
                            cancellationToken
                        );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetProfileSummary")
            .WithSummary("Get aggregated profile analytics summary")
            .WithDescription(
                "Returns overview/engagement/interaction statistics, audience distributions, and recent posts in a single response. Each section is independently cached.")
            .WithBrowserCache(60);
    }
}
