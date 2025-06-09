using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramAudienceService
{
    Task<Result<GenderRatio>> GetAudienceGenderPercentage(
        string accessToken,
        string instagramAccountId,
        CancellationToken cancellationToken = default
    );

    Task<Result<LocationRatio>> GetAudienceTopLocations(
        string accessToken,
        string instagramAccountId,
        LocationType locationType,
        CancellationToken cancellationToken = default
    );

    Task<Result<AgeRatio>> GetAudienceAgesPercentage(
        string accessToken,
        string instagramAccountId,
        CancellationToken cancellationToken = default
    );

    Task<Result<ReachRatio>> GetAudienceReachPercentage(
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    );
}
