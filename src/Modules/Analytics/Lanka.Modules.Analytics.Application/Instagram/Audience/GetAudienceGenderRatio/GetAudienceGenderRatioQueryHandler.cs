using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceGenderRatio;

internal sealed class GetAudienceGenderRatioQueryHandler
    : IQueryHandler<GetAudienceGenderRatioQuery, GenderRatio>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramAudienceService _instagramAudienceService;


    public GetAudienceGenderRatioQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramAudienceService instagramAudienceService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceService = instagramAudienceService;
    }

    public async Task<Result<GenderRatio>> Handle(
        GetAudienceGenderRatioQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<GenderRatio>(InstagramAccountErrors.NotFound);
        }

        return await this._instagramAudienceService.GetAudienceGenderPercentage(
            account.Token!.AccessToken.Value,
            account.Metadata.Id,
            cancellationToken
        );
    }
}
