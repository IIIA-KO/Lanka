using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Audience;

public sealed class LocationDistribution : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }

    public LocationType LocationType { get; set; }

    public LocationPercentage[] TopLocationPercentages { get; set; } = [];

    public LocationDistribution() { }

    public LocationDistribution(UserActivityLevel userActivityLevel)
        : base(GetTtlForActivityLevel(userActivityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "LocationRatio.InvalidData",
        "The provided data for audience location ratio is invalid."
    );

    public static Result<LocationDistribution> Create(JsonElement json, UserActivityLevel userActivityLevel)
    {
        if (!InstagramJsonService.HasData(json))
        {
            return Result.Failure<LocationDistribution>(InvalidData);
        }

        LocationPercentage[] topLocationPercentages =
            InstagramJsonService.ParseDemographicBreakdownWithPercentage(
                json,
                (location, percentage) =>
                    new LocationPercentage { Location = location, Percentage = percentage }
            );

        return new LocationDistribution(userActivityLevel)
        {
            TopLocationPercentages = topLocationPercentages
        };
    }
}

public sealed class LocationPercentage
{
    public string Location { get; init; }

    public double Percentage { get; init; }
}

public enum LocationType
{
    City = 1,
    Country = 2
}
