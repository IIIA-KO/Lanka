using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetEngagementStatistics;

public sealed class EngagementStatisticsResponse
{
    public string StatisticsPeriod { get; init; }
    public double ReachRate { get; init; }
    public double EngagementRate { get; init; }
    public double ERReach { get; init; }

    public static EngagementStatisticsResponse FromEngagementStatistics(EngagementStatistics engagementStatistics)
    {
        return new EngagementStatisticsResponse
        {
            StatisticsPeriod = engagementStatistics.StatisticsPeriod.ToString(),
            ReachRate = engagementStatistics.ReachRate,
            EngagementRate = engagementStatistics.EngagementRate,
            ERReach = engagementStatistics.ERReach,
        };
    }
}
