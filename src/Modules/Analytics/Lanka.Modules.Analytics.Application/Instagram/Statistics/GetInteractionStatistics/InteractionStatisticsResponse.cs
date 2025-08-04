using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetInteractionStatistics;

public sealed class InteractionStatisticsResponse
{
    public string StatisticsPeriod { get; init; }
    public double EngagementRate { get; init; }
    public double AverageLikes { get; init; }
    public double AverageComments { get; init; }
    public double CPE { get; init; }

    public static InteractionStatisticsResponse FromEngagementStatistics(InteractionStatistics interactionStatistics)
    {
        return new InteractionStatisticsResponse
        {
            StatisticsPeriod = interactionStatistics.StatisticsPeriod.ToString(),
            EngagementRate = interactionStatistics.EngagementRate,
            AverageLikes = interactionStatistics.AverageLikes,
            AverageComments = interactionStatistics.AverageComments,
            CPE = interactionStatistics.CPE,
        };
    }
}
