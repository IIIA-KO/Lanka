using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramAudienceService
{
    Task<Result<GenderDistribution>> GetAudienceGenderPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default
    );

    Task<Result<LocationDistribution>> GetAudienceTopLocations(
        InstagramAccount instagramAccount,
        LocationType locationType,
        CancellationToken cancellationToken = default
    );

    Task<Result<AgeDistribution>> GetAudienceAgesPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default
    );

    Task<Result<ReachDistribution>> GetAudienceReachPercentage(
        InstagramAccount instagramAccount,
        StatisticsPeriod period,
        CancellationToken cancellationToken = default
    );
}
