using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetInteractionStatistics;

internal sealed class GetInteractionStatisticsQueryHandler
    : IQueryHandler<GetInteractionStatisticsQuery, InteractionStatisticsResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramServiceFactory<IInstagramStatisticsService> _instagramStatisticsServiceFactory;

    public GetInteractionStatisticsQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramServiceFactory<IInstagramStatisticsService> instagramStatisticsServiceFactory
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramStatisticsServiceFactory = instagramStatisticsServiceFactory;
    }

    public async Task<Result<InteractionStatisticsResponse>> Handle(
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
            return Result.Failure<InteractionStatisticsResponse>(InstagramAccountErrors.NotFound);
        }

        IInstagramStatisticsService statisticsService = this._instagramStatisticsServiceFactory
            .GetService(account.Email.Value);

        Result<InteractionStatistics> result = await statisticsService.GetInteractionsStatistics(
            account,
            request.StatisticsPeriod,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return Result.Failure<InteractionStatisticsResponse>(result.Error);
        }
        
        return InteractionStatisticsResponse.FromEngagementStatistics(result.Value);
    }
}
