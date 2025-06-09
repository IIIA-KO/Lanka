using System.Text.Json;
using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

public sealed class OverviewStatistics
{
    private static Error InvalidData => Error.Validation(
        "OverviewStatistics.InvalidData",
        "Invalid data provided for overview statistics calculation."
    );

    public List<TotalValueMetricData> Metrics { get; set; } = [];

    public static Result<OverviewStatistics> ParseOverviewStatistics(JsonElement response)
    {
        if (!InstagramJsonHelper.HasData(response))
        {
            return Result.Failure<OverviewStatistics>(InvalidData);
        }

        var result = new OverviewStatistics();

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
