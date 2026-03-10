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
    private readonly IInstagramServiceFactory<IInstagramAudienceService> _instagramAudienceServiceFactory;

    public GetGenderDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramAudienceService> instagramAudienceServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceServiceFactory = instagramAudienceServiceFactory;
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

        IInstagramAudienceService audienceService = this._instagramAudienceServiceFactory
            .GetService(account.Email.Value);

        Result<GenderDistribution> result = await audienceService.GetAudienceGenderPercentage(
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
