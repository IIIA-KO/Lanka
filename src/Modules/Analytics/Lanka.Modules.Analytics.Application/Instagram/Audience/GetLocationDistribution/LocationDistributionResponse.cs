using Lanka.Modules.Analytics.Domain.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;

public sealed class LocationDistributionResponse
{
    public string LocationType { get; init; }
    
    public IReadOnlyList<LocationPercentage> LocationPercentages { get; init; } = [];

    public static LocationDistributionResponse FromLocationDistribution(LocationDistribution locationDistribution)
    {
        return new LocationDistributionResponse
        {
            LocationType = locationDistribution.LocationType.ToString(),
            LocationPercentages = locationDistribution.TopLocationPercentages,
        };
    }
}
