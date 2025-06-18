using System.Text.Json;
using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

public sealed record EngagementStatistics(
    double ReachRate,
    double EngagementRate,
    double ERReach
)
{
    private static Error InvalidData => Error.Validation(
        "Engagement.InvalidData",
        "Invalid data provided for engagement statistics calculation."
    );

    public static Result<EngagementStatistics> ParseEngagementsStatistics(
        JsonElement response,
        int followersCount
    )
    {
        if (!InstagramJsonHelper.HasData(response))
        {
            return Result.Failure<EngagementStatistics>(InvalidData);
        }

        int reach = InstagramJsonHelper.ParseMetricTotalValue(response, "reach");
        int likes = InstagramJsonHelper.ParseMetricTotalValue(response, "likes");
        int comments = InstagramJsonHelper.ParseMetricTotalValue(response, "comments");
        int saves = InstagramJsonHelper.ParseMetricTotalValue(response, "saves");

        double totalEngagements = likes + comments + saves;

        if (reach == 0 || followersCount <= 0)
        {
            return Result.Failure<EngagementStatistics>(InvalidData);
        }

        double reachRate = (double)reach / followersCount * 100;
        double engagementRate = totalEngagements / followersCount * 100;
        double erReach = totalEngagements / reach * 100;

        return new EngagementStatistics(reachRate, engagementRate, erReach);
    }
}
