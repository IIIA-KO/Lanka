using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Statistics;

public sealed class OverviewStatistics : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }
    public StatisticsPeriod StatisticsPeriod { get; set; }
    public List<TotalValueMetricData> Metrics { get; set; } = [];

    public OverviewStatistics() { }

    public OverviewStatistics(UserActivityLevel userActivityLevel)
        : base(GetTtlForActivityLevel(userActivityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "OverviewStatistics.InvalidData",
        "Invalid data provided for overview statistics calculation."
    );

    public static Result<OverviewStatistics> Create(JsonElement response, UserActivityLevel userActivityLevel)
    {
        if (!InstagramJsonService.HasData(response))
        {
            return Result.Failure<OverviewStatistics>(InvalidData);
        }

        var result = new OverviewStatistics(userActivityLevel);

        foreach (JsonElement metricElement in response.GetProperty("data").EnumerateArray())
        {
            if (!metricElement.TryGetProperty("name", out JsonElement nameProp)
                || !metricElement.TryGetProperty("total_value", out JsonElement totalValueProp)
                || !totalValueProp.TryGetProperty("value", out JsonElement valueProp)
               )
            {
                return Result.Failure<OverviewStatistics>(InvalidData);
            }

            var metricData = new TotalValueMetricData
            {
                Name = nameProp.GetString()!,
                Value = valueProp.GetInt32()
            };

            result.Metrics.Add(metricData);
        }

        return result;
    }
}

public sealed class TotalValueMetricData
{
    public string Name { get; set; }

    public int Value { get; set; }
}
