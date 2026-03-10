using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetEngagementStatistics;
using Lanka.Modules.Analytics.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Statistics;

internal sealed class GetBloggerEngagementStatistics : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("statistics/{userId:guid}/engagement"),
                async (
                    [FromRoute] Guid userId,
                    [FromQuery] StatisticsPeriod period,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<EngagementStatisticsResponse> result = await sender.Send(
                        new GetEngagementStatisticsQuery(userId, period),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetBloggerEngagementStatistics")
            .WithSummary("Get blogger engagement statistics")
            .WithDescription("Retrieves engagement statistics for a specific blogger's Instagram account within specified period");
    }
}
