using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;
using Lanka.Modules.Analytics.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Audience;

internal sealed class GetBloggerReachDistribution : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("audience/{userId:guid}/reach-distribution"),
                async (
                    [FromRoute] Guid userId,
                    [FromQuery] StatisticsPeriod period,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<ReachDistributionResponse> result = await sender.Send(
                        new GetReachDistributionQuery(userId, period),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetBloggerReachDistribution")
            .WithSummary("Get blogger audience reach distribution")
            .WithDescription("Retrieves reach distribution data for a specific blogger's Instagram account audience within specified period");
    }
}
