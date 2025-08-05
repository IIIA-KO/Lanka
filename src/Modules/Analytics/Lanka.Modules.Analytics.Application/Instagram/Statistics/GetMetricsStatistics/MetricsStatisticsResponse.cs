using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetMetricsStatistics;

public sealed class MetricsStatisticsResponse
{
    public string StatisticsPeriod { get; init; }
    public IReadOnlyList<TimeSeriesMetricData> Metrics { get; init; } = [];

    public static MetricsStatisticsResponse FromMetricStatistics(MetricsStatistics metrics)
    {
        return new MetricsStatisticsResponse
        {
            StatisticsPeriod = metrics.StatisticsPeriod.ToString(),
            Metrics = metrics.Metrics,
        };
    }
}
