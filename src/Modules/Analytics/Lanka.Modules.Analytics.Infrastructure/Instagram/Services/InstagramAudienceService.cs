using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Instagram.UserActivity;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.Domain.UserActivities;
using Lanka.Modules.Analytics.Infrastructure.Audience;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramAudienceService : IInstagramAudienceService
{
    private readonly IInstagramAudienceApi _instagramAudienceApi;
    private readonly AgeDistributionRepository _ageDistributionRepository;
    private readonly GenderDistributionRepository _genderDistributionRepository;
    private readonly LocationDistributionRepository _locationDistributionRepository;
    private readonly ReachDistributionRepository _reachDistributionRepository;
    private readonly IUserActivityService _userActivityService;

    public InstagramAudienceService(
        IInstagramAudienceApi instagramAudienceApi,
        AgeDistributionRepository ageDistributionRepository,
        GenderDistributionRepository genderDistributionRepository,
        LocationDistributionRepository locationDistributionRepository,
        ReachDistributionRepository reachDistributionRepository,
        IUserActivityService userActivityService
    )
    {
        this._instagramAudienceApi = instagramAudienceApi;
        this._ageDistributionRepository = ageDistributionRepository;
        this._genderDistributionRepository = genderDistributionRepository;
        this._locationDistributionRepository = locationDistributionRepository;
        this._reachDistributionRepository = reachDistributionRepository;
        this._userActivityService = userActivityService;
    }

    public async Task<Result<AgeDistribution>> GetAudienceAgesPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default
    )
    {
        AgeDistribution? validData =
            await this._ageDistributionRepository.GetValidAsync(instagramAccount.Id.Value, cancellationToken);

        if (validData is not null)
        {
            return validData;
        }

        AgeDistribution? fallbackData =
            await this._ageDistributionRepository.GetAsync(instagramAccount.Id.Value, cancellationToken);

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<AgeDistribution>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        try
        {
            JsonElement response = await this._instagramAudienceApi.GetAudienceInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "follower_demographics",
                period: "lifetime",
                metricType: "total_value",
                breakdown: "age",
                accessToken: accessToken,
                cancellationToken
            );

            if (response.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<AgeDistribution>(AgeDistribution.InvalidData);
            }

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<AgeDistribution> result = AgeDistribution.Create(response, userActivityLevel);

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            AgeDistribution ageDistribution = result.Value;
            ageDistribution.InstagramAccountId = instagramAccount.Id.Value;

            await this._ageDistributionRepository.ReplaceAsync(ageDistribution, cancellationToken);

            return ageDistribution;
        }
        catch
        {
            return fallbackData ?? Result.Failure<AgeDistribution>(AgeDistribution.InvalidData);
        }
    }

    public async Task<Result<GenderDistribution>> GetAudienceGenderPercentage(
        InstagramAccount instagramAccount,
        CancellationToken cancellationToken = default
    )
    {
        GenderDistribution? validData =
            await this._genderDistributionRepository.GetValidAsync(instagramAccount.Id.Value, cancellationToken);

        if (validData is not null)
        {
            return validData;
        }

        GenderDistribution? fallbackData =
            await this._genderDistributionRepository.GetAsync(instagramAccount.Id.Value, cancellationToken);

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<GenderDistribution>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        try
        {
            JsonElement response = await this._instagramAudienceApi.GetAudienceInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "follower_demographics",
                period: "lifetime",
                metricType: "total_value",
                breakdown: "gender",
                accessToken: accessToken,
                cancellationToken
            );

            if (response.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<GenderDistribution>(GenderDistribution.InvalidData);
            }

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<GenderDistribution> result = GenderDistribution.Create(response, userActivityLevel);

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            GenderDistribution genderDistribution = result.Value;
            genderDistribution.InstagramAccountId = instagramAccount.Id.Value;

            await this._genderDistributionRepository.ReplaceAsync(genderDistribution, cancellationToken);

            return validData;
        }
        catch
        {
            return fallbackData ?? Result.Failure<GenderDistribution>(GenderDistribution.InvalidData);
        }
    }


    public async Task<Result<LocationDistribution>> GetAudienceTopLocations(
        InstagramAccount instagramAccount,
        LocationType locationType,
        CancellationToken cancellationToken = default
    )
    {
        LocationDistribution? validData =
            await this._locationDistributionRepository.GetValidAsync(
                instagramAccount.Id.Value,
                locationType,
                cancellationToken
            );

        if (validData is not null)
        {
            return validData;
        }

        LocationDistribution? fallbackData =
            await this._locationDistributionRepository.GetAsync(
                instagramAccount.UserId.Value,
                locationType,
                cancellationToken
            );

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<LocationDistribution>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        try
        {
            JsonElement response = await this._instagramAudienceApi.GetAudienceInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "follower_demographics",
                period: "lifetime",
                metricType: "total_value",
                breakdown: locationType == LocationType.City ? "city" : "country",
                accessToken: accessToken,
                cancellationToken
            );

            if (response.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<LocationDistribution>(LocationDistribution.InvalidData);
            }

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<LocationDistribution> result = LocationDistribution.Create(response, userActivityLevel);

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            LocationDistribution locationDistribution = result.Value;
            locationDistribution.LocationType = locationType;
            locationDistribution.InstagramAccountId = instagramAccount.Id.Value;

            await this._locationDistributionRepository.ReplaceAsync(
                locationDistribution,
                locationType,
                cancellationToken
            );

            return locationDistribution;
        }
        catch
        {
            return Result.Failure<LocationDistribution>(LocationDistribution.InvalidData);
        }
    }

    public async Task<Result<ReachDistribution>> GetAudienceReachPercentage(
        InstagramAccount instagramAccount,
        StatisticsPeriod period,
        CancellationToken cancellationToken = default
    )
    {
        ReachDistribution? validData =
            await this._reachDistributionRepository.GetValidAsync(
                instagramAccount.Id.Value,
                period,
                cancellationToken
            );

        if (validData is not null)
        {
            return validData;
        }

        ReachDistribution? fallbackData =
            await this._reachDistributionRepository.GetAsync(
                instagramAccount.Id.Value,
                period,
                cancellationToken
            );

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<ReachDistribution>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        var dateRange = new InstagramPeriodCalculator(period);

        try
        {
            JsonElement response = await this._instagramAudienceApi.GetReachInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "reach",
                period: "day",
                since: dateRange.Since.ToString(CultureInfo.InvariantCulture),
                until: dateRange.Until.ToString(CultureInfo.InvariantCulture),
                breakdown: "follow_type",
                metricType: "total_value",
                accessToken: accessToken,
                cancellationToken
            );

            if (response.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<ReachDistribution>(ReachDistribution.InvalidData);
            }

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<ReachDistribution> result = ReachDistribution.Create(response, userActivityLevel);

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            ReachDistribution reachDistribution = result.Value;
            reachDistribution.InstagramAccountId = instagramAccount.Id.Value;
            reachDistribution.StatisticsPeriod = period;

            await this._reachDistributionRepository.ReplaceAsync(reachDistribution, period, cancellationToken);

            return reachDistribution;
        }
        catch
        {
            return Result.Failure<ReachDistribution>(ReachDistribution.InvalidData);
        }
    }
}
