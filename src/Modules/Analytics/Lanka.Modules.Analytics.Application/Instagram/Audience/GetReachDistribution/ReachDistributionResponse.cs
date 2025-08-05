using Lanka.Modules.Analytics.Domain.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;

public sealed class ReachDistributionResponse
{
    public string StatisticsPeriod { get; init; }
    
    public IReadOnlyList<ReachPercentage> ReachPercentages { get; init; } = [];

    public static ReachDistributionResponse FromReachDistribution(ReachDistribution reachDistribution)
    {
        return new ReachDistributionResponse
        {
            StatisticsPeriod = reachDistribution.StatisticsPeriod.ToString(),
            ReachPercentages = reachDistribution.ReachPercentages,
        };
    }
}
