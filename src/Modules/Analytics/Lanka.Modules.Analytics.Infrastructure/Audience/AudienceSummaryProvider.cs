using Lanka.Modules.Analytics.Application.Abstractions.Audience;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Infrastructure.Statistics;

namespace Lanka.Modules.Analytics.Infrastructure.Audience;

internal sealed class AudienceSummaryProvider : IAudienceSummaryProvider
{
    private readonly EngagementRepository _engagementRepository;
    private readonly AgeDistributionRepository _ageDistributionRepository;
    private readonly GenderDistributionRepository _genderDistributionRepository;
    private readonly LocationDistributionRepository _locationDistributionRepository;

    public AudienceSummaryProvider(
        EngagementRepository engagementRepository,
        AgeDistributionRepository ageDistributionRepository,
        GenderDistributionRepository genderDistributionRepository,
        LocationDistributionRepository locationDistributionRepository
    )
    {
        this._engagementRepository = engagementRepository;
        this._ageDistributionRepository = ageDistributionRepository;
        this._genderDistributionRepository = genderDistributionRepository;
        this._locationDistributionRepository = locationDistributionRepository;
    }

    public async Task<AudienceSummary?> GetSummaryAsync(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        Task<EngagementStatistics?> engagementTask = this._engagementRepository.GetValidAsync(
            instagramAccountId, StatisticsPeriod.Day21, cancellationToken
        );

        Task<AgeDistribution?> ageTask = this._ageDistributionRepository.GetValidAsync(
            instagramAccountId, cancellationToken
        );

        Task<GenderDistribution?> genderTask = this._genderDistributionRepository.GetValidAsync(
            instagramAccountId, cancellationToken
        );

        Task<LocationDistribution?> locationTask = this._locationDistributionRepository.GetValidAsync(
            instagramAccountId, LocationType.Country, cancellationToken
        );

        await Task.WhenAll(engagementTask, ageTask, genderTask, locationTask);

        EngagementStatistics? engagement = await engagementTask;
        AgeDistribution? ageDistribution = await ageTask;
        GenderDistribution? genderDistribution = await genderTask;
        LocationDistribution? locationDistribution = await locationTask;

        if (engagement is null 
            && ageDistribution is null 
            && genderDistribution is null 
            && locationDistribution is null)
        {
            return null;
        }

        AgePercentage? topAge = ageDistribution?.AgePercentages
            .OrderByDescending(a => a.Percentage)
            .FirstOrDefault();

        GenderPercentage? topGender = genderDistribution?.GenderPercentages
            .OrderByDescending(g => g.Percentage)
            .FirstOrDefault();

        LocationPercentage? topCountry = locationDistribution?.TopLocationPercentages
            .OrderByDescending(l => l.Percentage)
            .FirstOrDefault();

        return new AudienceSummary(
            EngagementRate: engagement?.EngagementRate,
            TopAgeGroup: topAge?.AgeGroup,
            TopGender: topGender?.Gender,
            TopGenderPercentage: topGender?.Percentage,
            TopCountry: topCountry?.Location,
            TopCountryPercentage: topCountry?.Percentage
        );
    }
}
