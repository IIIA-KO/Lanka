using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceLocationRatio;

internal sealed class GetAudienceLocationRatioQueryHandler
    : IQueryHandler<GetAudienceLocationRatioQuery, LocationRatio>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramAudienceService _instagramAudienceService;

    public GetAudienceLocationRatioQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramAudienceService instagramAudienceService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceService = instagramAudienceService;
    }

    public async Task<Result<LocationRatio>> Handle(
        GetAudienceLocationRatioQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<LocationRatio>(InstagramAccountErrors.NotFound);
        }

        return await this._instagramAudienceService.GetAudienceTopLocations(
            account.Token!.AccessToken.Value,
            account.Metadata.Id,
            request.LocationType,
            cancellationToken
        );
    }
}
