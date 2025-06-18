using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

public record LocationRatio(LocationPercentage[] TopLocationPercentages)
{
    private static Error InvalidData => Error.Validation(
        "LocationRatio.InvalidData",
        "The provided data for audience location ratio is invalid."
    );

    public static Result<LocationRatio> FromJson(JsonElement json)
    {
        if (!InstagramJsonHelper.HasData(json))
        {
            return Result.Failure<LocationRatio>(InvalidData);
        }

        LocationPercentage[] topLocationPercentages =
            InstagramJsonHelper.ParseDemographicBreakdownWithPercentage(
                json,
                (location, percentage) => 
                    new LocationPercentage { Location = location, Percentage = percentage }
            );

        return new LocationRatio(topLocationPercentages);
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
