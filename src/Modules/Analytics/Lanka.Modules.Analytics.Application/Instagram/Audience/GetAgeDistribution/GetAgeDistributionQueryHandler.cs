using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;

internal sealed class GetAgeDistributionQueryHandler
    : IQueryHandler<GetAgeDistributionQuery, AgeDistributionResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramAudienceService _instagramAudienceService;

    public GetAgeDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramAudienceService instagramAudienceService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceService = instagramAudienceService;
    }

    public async Task<Result<AgeDistributionResponse>> Handle(
        GetAgeDistributionQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<AgeDistributionResponse>(InstagramAccountErrors.NotFound);
        }

        Result<AgeDistribution> result = await this._instagramAudienceService.GetAudienceAgesPercentage(
            account,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Result.Failure<AgeDistributionResponse>(result.Error);
        }

        return AgeDistributionResponse.FromAgeDistribution(result.Value);
    }
}
