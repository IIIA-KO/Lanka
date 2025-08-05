using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;

public sealed class OverviewStatisticsResponse
{
    public string StatisticsPeriod { get; init; }

    public IReadOnlyList<TotalValueMetricData> Metrics { get; init; } = [];

    public static OverviewStatisticsResponse FromOverviewStatistics(OverviewStatistics overviewStatistics)
    {
        return new OverviewStatisticsResponse
        {
            StatisticsPeriod = overviewStatistics.StatisticsPeriod.ToString(),
            Metrics = overviewStatistics.Metrics,
        };
    }
}
