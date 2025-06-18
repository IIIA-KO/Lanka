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
    
    public async Task<Result<GenderRatio>> GetAudienceGenderPercentage(
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
            ? GenderRatio.FromJson(response)
            : Result.Failure<GenderRatio>(InstagramAccountErrors.Unexpected);
    }
    
    
    public async Task<Result<LocationRatio>> GetAudienceTopLocations(
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
            ? LocationRatio.FromJson(response)
            : Result.Failure<LocationRatio>(InstagramAccountErrors.Unexpected);
    }

    public async Task<Result<AgeRatio>> GetAudienceAgesPercentage(
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
            ? AgeRatio.FromJson(response)
            : Result.Failure<AgeRatio>(InstagramAccountErrors.Unexpected);
    }

    public async Task<Result<ReachRatio>> GetAudienceReachPercentage(
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
            ? ReachRatio.FromJson(response)
            : Result.Failure<ReachRatio>(InstagramAccountErrors.Unexpected);
    }
}
