using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

public sealed record AgeRatio(AgePercentage[] AgePercentages)
{
    private static Error InvalidData => Error.Validation(
        "AgeRatio.InvalidData",
        "The provided data for audience age ratio is invalid."
    );

    public static Result<AgeRatio> FromJson(JsonElement json)
    {
        if (!InstagramJsonHelper.HasData(json))
        {
            return Result.Failure<AgeRatio>(InvalidData);
        }

        AgePercentage[] agePercentages = InstagramJsonHelper.ParseDemographicBreakdownWithPercentage(
            json,
            (ageGroup, percentage) =>
                new AgePercentage { AgeGroup = ageGroup, Percentage = percentage }
        );

        return new AgeRatio(agePercentages);
    }
}

public sealed class AgePercentage
{
    public string AgeGroup { get; init; }

    public double Percentage { get; init; }
}
