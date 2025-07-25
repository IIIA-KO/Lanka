using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

public sealed record AgeDistribution(AgePercentage[] AgePercentages)
{
    private static Error InvalidData => Error.Validation(
        "AgeRatio.InvalidData",
        "The provided data for audience age ratio is invalid."
    );

    public static Result<AgeDistribution> FromJson(JsonElement json)
    {
        if (!InstagramJsonHelper.HasData(json))
        {
            return Result.Failure<AgeDistribution>(InvalidData);
        }

        AgePercentage[] agePercentages = InstagramJsonHelper.ParseDemographicBreakdownWithPercentage(
            json,
            (ageGroup, percentage) =>
                new AgePercentage { AgeGroup = ageGroup, Percentage = percentage }
        );

        return new AgeDistribution(agePercentages);
    }
}

public sealed class AgePercentage
{
    public string AgeGroup { get; init; }

    public double Percentage { get; init; }
}
