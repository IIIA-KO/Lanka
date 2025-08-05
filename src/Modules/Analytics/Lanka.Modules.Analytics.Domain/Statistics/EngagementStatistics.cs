using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Statistics;

public sealed class EngagementStatistics : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }

    public StatisticsPeriod StatisticsPeriod { get; set; }

    public double ReachRate { get; set; }
    public double EngagementRate { get; set; }
    public double ERReach { get; set; }

    public EngagementStatistics() { }

    public EngagementStatistics(UserActivityLevel userActivityLevel)
        : base(GetTtlForActivityLevel(userActivityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "Engagement.InvalidData",
        "Invalid data provided for engagement statistics calculation."
    );

    public static Result<EngagementStatistics> Create(
        JsonElement response,
        int followersCount,
        UserActivityLevel userActivityLevel
    )
    {
        if (!InstagramJsonService.HasData(response))
        {
            return Result.Failure<EngagementStatistics>(InvalidData);
        }

        int reach = InstagramJsonService.ParseMetricTotalValue(response, "reach");
        int likes = InstagramJsonService.ParseMetricTotalValue(response, "likes");
        int comments = InstagramJsonService.ParseMetricTotalValue(response, "comments");
        int saves = InstagramJsonService.ParseMetricTotalValue(response, "saves");

        double totalEngagements = likes + comments + saves;

        if (reach == 0 || followersCount <= 0)
        {
            return Result.Failure<EngagementStatistics>(InvalidData);
        }

        double reachRate = (double)reach / followersCount * 100;
        double engagementRate = totalEngagements / followersCount * 100;
        double erReach = totalEngagements / reach * 100;

        return new EngagementStatistics(userActivityLevel)
        {
            ReachRate = reachRate,
            EngagementRate = engagementRate,
            ERReach = erReach
        };
    }
}
