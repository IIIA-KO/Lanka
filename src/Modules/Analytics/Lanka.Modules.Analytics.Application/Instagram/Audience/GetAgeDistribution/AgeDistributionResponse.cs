using Lanka.Modules.Analytics.Domain.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;

public sealed class AgeDistributionResponse
{
    public IReadOnlyList<AgePercentage> AgePercentages { get; init; } = [];

    public static AgeDistributionResponse FromAgeDistribution(AgeDistribution ageDistribution)
    {
        return new AgeDistributionResponse
        {
            AgePercentages = ageDistribution.AgePercentages,
        };
    }
}
