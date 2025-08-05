using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;

internal sealed class GetLocationDistributionQueryHandler
    : IQueryHandler<GetLocationDistributionQuery, LocationDistributionResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramAudienceService _instagramAudienceService;

    public GetLocationDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramAudienceService instagramAudienceService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceService = instagramAudienceService;
    }

    public async Task<Result<LocationDistributionResponse>> Handle(
        GetLocationDistributionQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<LocationDistributionResponse>(InstagramAccountErrors.NotFound);
        }

        Result<LocationDistribution> result = await this._instagramAudienceService.GetAudienceTopLocations(
            account,
            request.LocationType,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Result.Failure<LocationDistributionResponse>(result.Error);
        }

        return LocationDistributionResponse.FromLocationDistribution(result.Value);
    }
}
