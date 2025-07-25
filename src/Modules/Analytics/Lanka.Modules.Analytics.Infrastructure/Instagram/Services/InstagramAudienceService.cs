using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramAudienceService : IInstagramAudienceService
{
    private readonly IInstagramAudienceApi _instagramAudienceApi;

    public InstagramAudienceService(IInstagramAudienceApi instagramAudienceApi)
    {
        this._instagramAudienceApi = instagramAudienceApi;
    }
    
    public async Task<Result<GenderDistribution>> GetAudienceGenderPercentage(
        string accessToken,
        string instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement response = await this._instagramAudienceApi.GetAudienceInsightsAsync(
            instagramAccountId,
            metric: "follower_demographics",
            period: "lifetime",
            metricType: "total_value",
            breakdown: "gender",
            accessToken,
            cancellationToken
        );

        return response.ValueKind != JsonValueKind.Undefined
            ? GenderDistribution.FromJson(response)
            : Result.Failure<GenderDistribution>(InstagramAccountErrors.Unexpected);
    }
    
    
    public async Task<Result<LocationDistribution>> GetAudienceTopLocations(
        string accessToken,
        string instagramAccountId,
        LocationType locationType,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement response = await this._instagramAudienceApi.GetAudienceInsightsAsync(
            instagramAccountId,
            metric: "follower_demographics",
            period: "lifetime",
            metricType: "total_value",
            breakdown: locationType == LocationType.City ? "city" : "country",
            accessToken,
            cancellationToken
        );

        return response.ValueKind != JsonValueKind.Undefined
            ? LocationDistribution.FromJson(response)
            : Result.Failure<LocationDistribution>(InstagramAccountErrors.Unexpected);
    }

    public async Task<Result<AgeDistribution>> GetAudienceAgesPercentage(
        string accessToken,
        string instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement response = await this._instagramAudienceApi.GetAudienceInsightsAsync(
            instagramAccountId,
            metric: "follower_demographics",
            period: "lifetime",
            metricType: "total_value",
            breakdown: "age",
            accessToken,
            cancellationToken
        );

        return response.ValueKind != JsonValueKind.Undefined
            ? AgeDistribution.FromJson(response)
            : Result.Failure<AgeDistribution>(InstagramAccountErrors.Unexpected);
    }

    public async Task<Result<ReachDistribution>> GetAudienceReachPercentage(
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement response = await this._instagramAudienceApi.GetReachInsightsAsync(
            request.InstagramAccountId,
            metric: "reach",
            period: "day",
            since: request.Since.ToString(CultureInfo.InvariantCulture),
            until: request.Until.ToString(CultureInfo.InvariantCulture),
            breakdown: "follow_type",
            metricType: "total_value",
            accessToken: request.AccessToken,
            cancellationToken
        );

        return response.ValueKind != JsonValueKind.Undefined
            ? ReachDistribution.FromJson(response)
            : Result.Failure<ReachDistribution>(InstagramAccountErrors.Unexpected);
    }
}
