using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Audience;

public sealed class GenderDistribution : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }

    public GenderPercentage[] GenderPercentages { get; set; } = [];

    public GenderDistribution() { }

    public GenderDistribution(UserActivityLevel activityLevel)
        : base(GetTtlForActivityLevel(activityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "GenderRatio.InvalidData",
        "The provided data for audience gender ratio is invalid."
    );

    public static Result<GenderDistribution> Create(JsonElement json, UserActivityLevel activityLevel)
    {
        if (!InstagramJsonService.HasData(json))
        {
            return Result.Failure<GenderDistribution>(InvalidData);
        }

        GenderPercentage[] genderPercentages =
            InstagramJsonService.ParseDemographicBreakdownWithPercentage(
                json,
                (gender, percentage) =>
                    new GenderPercentage { Gender = gender, Percentage = percentage }
            );

        return new GenderDistribution(activityLevel) { GenderPercentages = genderPercentages };
    }
}

public sealed class GenderPercentage
{
    public string Gender { get; init; }

    public double Percentage { get; init; }
}
