using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

public sealed class TableStatistics
{
    private static Error InvalidData => Error.Validation(
        "TableStatistics.InvalidData",
        "The provided data for table statistics is invalid."
    );

    public List<TimeSeriesMetricData> Metrics { get; set; } = [];

    public static Result<TableStatistics> ParseTableStatistics(JsonElement response)
    {
        if (!InstagramJsonHelper.HasData(response))
        {
            return Result.Failure<TableStatistics>(InvalidData);
        }

        var result = new TableStatistics();

        foreach (JsonElement metricElement in response.GetProperty("data").EnumerateArray())
        {
            if (!metricElement.TryGetProperty("name", out JsonElement nameProp)
                || !metricElement.TryGetProperty("values", out JsonElement valuesProp)
                || valuesProp.ValueKind != JsonValueKind.Array)
            {
                return Result.Failure<TableStatistics>(InvalidData);
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
                    return Result.Failure<TableStatistics>(InvalidData);
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
