using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;
using Lanka.Modules.Analytics.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Statistics;

internal sealed class GetBloggerOverviewStatistics : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("statistics/{userId:guid}/overview"),
                async (
                    [FromRoute] Guid userId,
                    [FromQuery] StatisticsPeriod period,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<OverviewStatisticsResponse> result = await sender.Send(
                        new GetOverviewStatisticsQuery(userId, period),
                        cancellationToken
                    );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetBloggerOverviewStatistics")
            .WithSummary("Get blogger overview statistics")
            .WithDescription("Retrieves overview statistics for a specific blogger's Instagram account within specified period");
    }
}
