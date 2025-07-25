using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramAudienceService
{
    Task<Result<GenderDistribution>> GetAudienceGenderPercentage(
        string accessToken,
        string instagramAccountId,
        CancellationToken cancellationToken = default
    );

    Task<Result<LocationDistribution>> GetAudienceTopLocations(
        string accessToken,
        string instagramAccountId,
        LocationType locationType,
        CancellationToken cancellationToken = default
    );

    Task<Result<AgeDistribution>> GetAudienceAgesPercentage(
        string accessToken,
        string instagramAccountId,
        CancellationToken cancellationToken = default
    );

    Task<Result<ReachDistribution>> GetAudienceReachPercentage(
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    );
}
