using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Common.Presentation.ApiResults;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetMetricsStatistics;
using Lanka.Modules.Analytics.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Modules.Analytics.Presentation.Statistics;

internal sealed class GetTableStatistics : AnalyticsEndpointBase
{
    protected override RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app)
    {
        return app.MapGet(this.BuildRoute("table-statistics"),
                async (
                    [FromQuery] StatisticsPeriod period,
                    ISender sender,
                    IUserContext userContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    Result<MetricsStatisticsResponse> result =
                        await sender.Send(
                            new GetMetricsStatisticsQuery(userContext.GetUserId(), period),
                            cancellationToken
                        );

                    return result.Match(Results.Ok, ApiResult.Problem);
                }
            )
            .WithTags(Tags.Analytics)
            .WithName("GetTableStatistics")
            .WithSummary("Get table statistics")
            .WithDescription("Retrieves detailed metrics statistics for Instagram account within specified period")
            .WithOpenApi();
    }
}
