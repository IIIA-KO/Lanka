using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetGenderDistribution;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;
using Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;
using Lanka.Modules.Analytics.Application.Instagram.GetPosts;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetEngagementStatistics;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetInteractionStatistics;
using Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Analytics.Application.Instagram.ProfileSummary;

internal sealed class GetProfileSummaryQueryHandler
    : IQueryHandler<GetProfileSummaryQuery, ProfileSummaryResponse>
{
    private const int RecentPostsLimit = 6;

    private readonly ISender _sender;
    private readonly ILogger<GetProfileSummaryQueryHandler> _logger;

    public GetProfileSummaryQueryHandler(
        ISender sender,
        ILogger<GetProfileSummaryQueryHandler> logger)
    {
        this._sender = sender;
        this._logger = logger;
    }

    public async Task<Result<ProfileSummaryResponse>> Handle(
        GetProfileSummaryQuery request,
        CancellationToken cancellationToken)
    {
        StatisticsPeriod period = request.StatisticsPeriod;
        Guid userId = request.UserId;

        // Sub-queries are dispatched sequentially: they share the request's DI scope
        // (and the scoped DbContext / IInstagramAccountRepository), so concurrent
        // dispatch would trigger EF Core's "second operation" exception. The cache
        // behaviour in front of every ICachedQuery makes repeat calls cheap anyway.
        return new ProfileSummaryResponse
        {
            Overview = await this.RunSafe(new GetOverviewStatisticsQuery(userId, period), cancellationToken),
            Engagement = await this.RunSafe(new GetEngagementStatisticsQuery(userId, period), cancellationToken),
            Interaction = await this.RunSafe(new GetInteractionStatisticsQuery(userId, period), cancellationToken),
            AgeDistribution = await this.RunSafe(new GetAgeDistributionQuery(userId), cancellationToken),
            GenderDistribution = await this.RunSafe(new GetGenderDistributionQuery(userId), cancellationToken),
            LocationCountry = await this.RunSafe(new GetLocationDistributionQuery(userId, LocationType.Country), cancellationToken),
            LocationCity = await this.RunSafe(new GetLocationDistributionQuery(userId, LocationType.City), cancellationToken),
            ReachDistribution = await this.RunSafe(new GetReachDistributionQuery(userId, period), cancellationToken),
            RecentPosts = await this.RunSafe(new GetPostsQuery(userId, RecentPostsLimit, null, null), cancellationToken)
        };
    }

    private async Task<TResponse?> RunSafe<TResponse>(
        IRequest<Result<TResponse>> query,
        CancellationToken cancellationToken) where TResponse : class
    {
        try
        {
            Result<TResponse> result = await this._sender.Send(query, cancellationToken);
            return result.IsSuccess ? result.Value : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            this._logger.LogWarning(exception,
                "Sub-query {Query} failed inside GetProfileSummaryQueryHandler. Section omitted from the response.",
                query.GetType().Name);
            return null;
        }
    }
}
