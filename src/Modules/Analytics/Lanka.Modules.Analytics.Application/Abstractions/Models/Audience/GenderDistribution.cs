using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

public sealed record GenderDistribution(GenderPercentage[] GenderPercentages)
{
    private static Error InvalidData => Error.Validation(
        "GenderRatio.InvalidData",
        "The provided data for audience gender ratio is invalid."
    );

    public static Result<GenderDistribution> FromJson(JsonElement json)
    {
        if (!InstagramJsonHelper.HasData(json))
        {
            return Result.Failure<GenderDistribution>(InvalidData);
        }

        GenderPercentage[] genderPercentages =
            InstagramJsonHelper.ParseDemographicBreakdownWithPercentage(
                json,
                (gender, percentage) =>
                    new GenderPercentage { Gender = gender, Percentage = percentage }
            );

        return new GenderDistribution(genderPercentages);
    }
}

public sealed class GenderPercentage
{
    public string Gender { get; init; }

    public double Percentage { get; init; }
}
