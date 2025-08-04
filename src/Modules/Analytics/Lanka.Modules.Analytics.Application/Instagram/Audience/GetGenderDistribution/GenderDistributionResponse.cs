using Lanka.Modules.Analytics.Domain.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetGenderDistribution;

public sealed class GenderDistributionResponse
{
    public IReadOnlyList<GenderPercentage> GenderPercentages { get; init; } = [];

    public static GenderDistributionResponse FromGenderDistribution(GenderDistribution genderDistribution)
    {
        return new GenderDistributionResponse
        {
            GenderPercentages = genderDistribution.GenderPercentages,
        };
    }
}
