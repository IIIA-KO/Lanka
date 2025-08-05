using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Statistics;

public sealed class InteractionStatistics : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }
    public StatisticsPeriod StatisticsPeriod { get; set; }
    public double EngagementRate { get; set; }
    public double AverageLikes { get; set; }
    public double AverageComments { get; set; }
    public double CPE { get; set; }

    public InteractionStatistics() { }

    public InteractionStatistics(UserActivityLevel userActivityLevel)
        : base(GetTtlForActivityLevel(userActivityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "InteractionStatistics.InvalidData",
        "Invalid data provided for interaction statistics calculation."
    );

    public static Result<InteractionStatistics> Create(
        JsonElement interactionResponse,
        JsonElement adsResponse,
        int daysCount,
        UserActivityLevel userActivityLevel
    )
    {
        if (!InstagramJsonService.HasData(interactionResponse)
            || !InstagramJsonService.HasData(adsResponse)
           )
        {
            return Result.Failure<InteractionStatistics>(InvalidData);
        }

        int interactions = InstagramJsonService.ParseMetricTotalValue(interactionResponse, "total_interactions");
        int reach = InstagramJsonService.ParseMetricTotalValue(interactionResponse, "reach");
        int likes = InstagramJsonService.ParseMetricTotalValue(interactionResponse, "likes");
        int comments = InstagramJsonService.ParseMetricTotalValue(interactionResponse, "comments");

        if (reach == 0 || daysCount <= 0)
        {
            return Result.Failure<InteractionStatistics>(InvalidData);
        }

        double engagementRate = (double)interactions / reach * 100;
        double averageLikes = (double)likes / daysCount;
        double averageComments = (double)comments / daysCount;

        double totalAdSpend = TryParseAdSpend(adsResponse);
        double cpe = interactions > 0 ? totalAdSpend / interactions : 0;

        return new InteractionStatistics(userActivityLevel)
        {
            EngagementRate = engagementRate,
            AverageLikes = averageLikes,
            AverageComments = averageComments,
            CPE = cpe
        };
    }

    private static double TryParseAdSpend(JsonElement adsResponse)
    {
        try
        {
            return double.Parse(
                adsResponse.GetProperty("data")[0].GetProperty("spend").GetString()!,
                CultureInfo.InvariantCulture
            );
        }
        catch
        {
            return 0;
        }
    }
}
