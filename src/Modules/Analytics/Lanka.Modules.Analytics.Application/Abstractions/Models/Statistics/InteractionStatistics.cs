using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

public sealed record InteractionStatistics(
    double EngagementRate,
    double AverageLikes,
    double AverageComments,
    double CPE
)
{
    private static Error InvalidData => Error.Validation(
        "InteractionStatistics.InvalidData",
        "Invalid data provided for interaction statistics calculation."
    );

    public static Result<InteractionStatistics> ParseInteractionStatistics(
        JsonElement interactionResponse,
        JsonElement adsResponse,
        int daysCount
    )
    {
        if (!InstagramJsonHelper.HasData(interactionResponse)
            || !InstagramJsonHelper.HasData(adsResponse)
           )
        {
            return Result.Failure<InteractionStatistics>(InvalidData);
        }

        int interactions = InstagramJsonHelper.ParseMetricTotalValue(interactionResponse, "total_interactions");
        int reach = InstagramJsonHelper.ParseMetricTotalValue(interactionResponse, "reach");
        int likes = InstagramJsonHelper.ParseMetricTotalValue(interactionResponse, "likes");
        int comments = InstagramJsonHelper.ParseMetricTotalValue(interactionResponse, "comments");

        if (reach == 0 || daysCount <= 0)
        {
            return Result.Failure<InteractionStatistics>(InvalidData);
        }

        double engagementRate = (double)interactions / reach * 100;
        double averageLikes = (double)likes / daysCount;
        double averageComments = (double)comments / daysCount;

        double totalAdSpend = TryParseAdSpend(adsResponse);
        double cpe = interactions > 0 ? totalAdSpend / interactions : 0;

        return new InteractionStatistics(engagementRate, averageLikes, averageComments, cpe);
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
