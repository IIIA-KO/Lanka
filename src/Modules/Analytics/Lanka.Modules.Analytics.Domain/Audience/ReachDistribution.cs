using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Audience;

public sealed class ReachDistribution : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }

    public StatisticsPeriod StatisticsPeriod { get; set; }

    public int TotalReach { get; set; }

    public ReachPercentage[] ReachPercentages { get; set; } = [];

    public ReachDistribution() { }

    public ReachDistribution(UserActivityLevel activityLevel)
        : base(GetTtlForActivityLevel(activityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "ReachRatio.InvalidData",
        "The provided data for audience reach ratio is invalid."
    );

    public static Result<ReachDistribution> Create(JsonElement json, UserActivityLevel userActivityLevel)
    {
        if (!InstagramJsonService.HasData(json))
        {
            return Result.Failure<ReachDistribution>(InvalidData);
        }

        int totalReach = json
            .GetProperty("data")[0]
            .GetProperty("total_value")
            .GetProperty("breakdowns")[0]
            .GetProperty("results")
            .EnumerateArray()
            .Sum(reach => reach.GetProperty("value").GetInt32());

        ReachPercentage[] reachPercentages = InstagramJsonService.ParseDemographicBreakdownWithPercentage(
            json,
            (followType, percentage) =>
                new ReachPercentage { FollowType = followType, Percentage = percentage }
        );

        return new ReachDistribution(userActivityLevel)
        {
            TotalReach = totalReach,
            ReachPercentages = reachPercentages
        };
    }
}

public class ReachPercentage
{
    public string FollowType { get; init; }

    public double Percentage { get; init; }
}
