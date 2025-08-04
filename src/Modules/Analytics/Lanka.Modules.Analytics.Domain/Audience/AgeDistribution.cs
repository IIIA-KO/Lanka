using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Audience;

public sealed class AgeDistribution : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }

    public AgePercentage[] AgePercentages { get; set; } = [];

    public AgeDistribution() { }

    public AgeDistribution(UserActivityLevel activityLevel)
        : base(GetTtlForActivityLevel(activityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "AgeRatio.InvalidData",
        "The provided data for audience age ratio is invalid."
    );

    public static Result<AgeDistribution> Create(JsonElement json, UserActivityLevel userActivityLevel)
    {
        if (!InstagramJsonService.HasData(json))
        {
            return Result.Failure<AgeDistribution>(InvalidData);
        }

        AgePercentage[] agePercentages = InstagramJsonService.ParseDemographicBreakdownWithPercentage(
            json,
            (ageGroup, percentage) =>
                new AgePercentage { AgeGroup = ageGroup, Percentage = percentage }
        );

        return new AgeDistribution(userActivityLevel) { AgePercentages = agePercentages };
    }
}

public sealed class AgePercentage
{
    public string AgeGroup { get; init; }

    public double Percentage { get; init; }
}
