using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetInteractionStatistics;

internal sealed class GetInteractionStatisticsQueryHandler
    : IQueryHandler<GetInteractionStatisticsQuery, InteractionStatistics>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramStatisticsService _instagramStatisticsService;

    public GetInteractionStatisticsQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramStatisticsService instagramStatisticsService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramStatisticsService = instagramStatisticsService;
    }

    public async Task<Result<InteractionStatistics>> Handle(
        GetInteractionStatisticsQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<InteractionStatistics>(InstagramAccountErrors.NotFound);
        }

        return await this._instagramStatisticsService.GetInteractionsStatistics(
            account.AdvertisementAccountId.Value,
            new InstagramPeriodRequest(
                account.Token!.AccessToken.Value,
                account.Metadata.Id,
                request.StatisticsPeriod
            ),
            cancellationToken
        );
    }
}
