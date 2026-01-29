using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramAudienceService : IInstagramAudienceService
{
    public Task<Result<GenderDistribution>> GetAudienceGenderPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default)
    {
        GenderDistribution distribution = FakeInstagramDataGenerator.GenerateGenderDistributionDomain(
            instagramAccount.Id.Value
        );

        return Task.FromResult((Result<GenderDistribution>)distribution);
    }

    public Task<Result<LocationDistribution>> GetAudienceTopLocations(
        InstagramAccount instagramAccount,
        LocationType locationType,
        CancellationToken cancellationToken = default)
    {
        LocationDistribution distribution = FakeInstagramDataGenerator.GenerateLocationDistributionDomain(
            instagramAccount.Id.Value,
            locationType
        );

        return Task.FromResult((Result<LocationDistribution>)distribution);
    }

    public Task<Result<AgeDistribution>> GetAudienceAgesPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default)
    {
        AgeDistribution distribution = FakeInstagramDataGenerator.GenerateAgeDistributionDomain(
            instagramAccount.Id.Value
        );

        return Task.FromResult((Result<AgeDistribution>)distribution);
    }

    public Task<Result<ReachDistribution>> GetAudienceReachPercentage(
        InstagramAccount instagramAccount,
        StatisticsPeriod period,
        CancellationToken cancellationToken = default)
    {
        ReachDistribution distribution = FakeInstagramDataGenerator.GenerateReachDistributionDomain(
            instagramAccount.Id.Value,
            period
        );

        return Task.FromResult((Result<ReachDistribution>)distribution);
    }
}
