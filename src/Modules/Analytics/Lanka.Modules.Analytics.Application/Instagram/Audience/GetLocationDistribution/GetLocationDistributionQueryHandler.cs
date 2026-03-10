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
    private readonly IInstagramServiceFactory<IInstagramAudienceService> _instagramAudienceServiceFactory;

    public GetLocationDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramAudienceService> instagramAudienceServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceServiceFactory = instagramAudienceServiceFactory;
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

        IInstagramAudienceService audienceService = this._instagramAudienceServiceFactory
            .GetService(account.Email.Value);

        Result<LocationDistribution> result = await audienceService.GetAudienceTopLocations(
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
