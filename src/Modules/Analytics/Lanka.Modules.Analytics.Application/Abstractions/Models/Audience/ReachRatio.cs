using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

public sealed record ReachRatio(int TotalReach, ReachPercentage[] ReachPercentages)
{
    private static Error InvalidData => Error.Validation(
        "ReachRatio.InvalidData",
        "The provided data for audience reach ratio is invalid."
    );

    public static Result<ReachRatio> FromJson(JsonElement json)
    {
        if (!InstagramJsonHelper.HasData(json))
        {
            return Result.Failure<ReachRatio>(InvalidData);
        }
        
        int totalReach = json
            .GetProperty("data")[0]
            .GetProperty("total_value")
            .GetProperty("breakdowns")[0]
            .GetProperty("results")
            .EnumerateArray()
            .Sum(reach => reach.GetProperty("value").GetInt32());
        
        ReachPercentage[] reachPercentages = InstagramJsonHelper.ParseDemographicBreakdownWithPercentage(
            json,
            (followType, percentage) => 
                new ReachPercentage { FollowType = followType, Percentage = percentage }
        );
        
        return new ReachRatio(totalReach, reachPercentages);
    }
}

public class ReachPercentage
{
    public string FollowType { get; init; }

    public double Percentage { get; init; }
}
