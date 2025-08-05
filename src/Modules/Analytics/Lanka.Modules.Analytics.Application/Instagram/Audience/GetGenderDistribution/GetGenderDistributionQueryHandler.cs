using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetGenderDistribution;

internal sealed class GetGenderDistributionQueryHandler
    : IQueryHandler<GetGenderDistributionQuery, GenderDistributionResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramAudienceService _instagramAudienceService;

    public GetGenderDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramAudienceService instagramAudienceService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceService = instagramAudienceService;
    }

    public async Task<Result<GenderDistributionResponse>> Handle(
        GetGenderDistributionQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<GenderDistributionResponse>(InstagramAccountErrors.NotFound);
        }

        Result<GenderDistribution> result = await this._instagramAudienceService.GetAudienceGenderPercentage(
            account,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Result.Failure<GenderDistributionResponse>(result.Error);
        }

        return GenderDistributionResponse.FromGenderDistribution(result.Value);
    }
}
