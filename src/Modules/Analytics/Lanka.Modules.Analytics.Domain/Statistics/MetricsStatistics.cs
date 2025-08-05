using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Statistics;

public sealed class MetricsStatistics : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }
    public StatisticsPeriod StatisticsPeriod { get; set; }
    public List<TimeSeriesMetricData> Metrics { get; set; } = [];

    public MetricsStatistics() { }

    public MetricsStatistics(UserActivityLevel userActivityLevel)
        : base(GetTtlForActivityLevel(userActivityLevel))
    {
    }

    public static Error InvalidData => Error.Validation(
        "MetricsStatistics.InvalidData",
        "The provided data for table statistics is invalid."
    );

    public static Result<MetricsStatistics> Create(JsonElement response, UserActivityLevel userActivityLevel)
    {
        if (!InstagramJsonService.HasData(response))
        {
            return Result.Failure<MetricsStatistics>(InvalidData);
        }

        var result = new MetricsStatistics(userActivityLevel);

        foreach (JsonElement metricElement in response.GetProperty("data").EnumerateArray())
        {
            if (!metricElement.TryGetProperty("name", out JsonElement nameProp)
                || !metricElement.TryGetProperty("values", out JsonElement valuesProp)
                || valuesProp.ValueKind != JsonValueKind.Array)
            {
                return Result.Failure<MetricsStatistics>(InvalidData);
            }

            var metricData = new TimeSeriesMetricData
            {
                Name = nameProp.GetString()!
            };

            foreach (JsonElement valueElement in valuesProp.EnumerateArray())
            {
                if (!valueElement.TryGetProperty("value", out JsonElement valueProp)
                    || !valueElement.TryGetProperty("end_time", out JsonElement endTimeProp)
                   )
                {
                    return Result.Failure<MetricsStatistics>(InvalidData);
                }

                int value = valueProp.GetInt32();
                DateTime endTime = ParseDateTime(endTimeProp.GetString()!);

                metricData.Values.Add(endTime, value);
            }

            result.Metrics.Add(metricData);
        }

        return result;
    }


    private static DateTime ParseDateTime(string dateTimeString)
    {
        return DateTimeOffset
            .ParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)
            .UtcDateTime;
    }
}

public sealed class TimeSeriesMetricData
{
    public string Name { get; set; }

    public Dictionary<DateTime, int> Values { get; set; } = [];
}
