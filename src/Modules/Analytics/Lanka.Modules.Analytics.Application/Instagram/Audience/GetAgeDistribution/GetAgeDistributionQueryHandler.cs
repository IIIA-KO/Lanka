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
    private readonly IInstagramServiceFactory<IInstagramAudienceService> _instagramAudienceServiceFactory;

    public GetAgeDistributionQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramAudienceService> instagramAudienceServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramAudienceServiceFactory = instagramAudienceServiceFactory;
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

        IInstagramAudienceService audienceService = this._instagramAudienceServiceFactory
            .GetService(account.Email.Value);

        Result<AgeDistribution> result = await audienceService.GetAudienceAgesPercentage(
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
