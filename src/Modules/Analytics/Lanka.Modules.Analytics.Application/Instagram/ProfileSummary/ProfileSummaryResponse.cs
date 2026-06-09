using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetGenderDistribution;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetEngagementStatistics;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetInteractionStatistics;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;

namespace Lanka.Modules.Analytics.Application.Instagram.ProfileSummary;

public sealed class ProfileSummaryResponse
{
    public OverviewStatisticsResponse? Overview { get; init; }
    public EngagementStatisticsResponse? Engagement { get; init; }
    public InteractionStatisticsResponse? Interaction { get; init; }
    public AgeDistributionResponse? AgeDistribution { get; init; }
    public GenderDistributionResponse? GenderDistribution { get; init; }
    public LocationDistributionResponse? LocationCountry { get; init; }
    public LocationDistributionResponse? LocationCity { get; init; }
    public ReachDistributionResponse? ReachDistribution { get; init; }
    public PostsResponse? RecentPosts { get; init; }
}
