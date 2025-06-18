using System.Text.Json;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

internal static class InstagramJsonHelper
{
    public static int ParseMetricTotalValue(JsonElement response, string metric)
    {
        return response
            .GetProperty("data")
            .EnumerateArray()
            .First(e => e.GetProperty("name").GetString() == metric)
            .GetProperty("total_value")
            .GetProperty("value")
            .GetInt32();
    }

    public static bool HasData(JsonElement response)
    {
        return response.TryGetProperty("data", out JsonElement data)
               && data.ValueKind == JsonValueKind.Array
               && data.EnumerateArray().Any();
    }
    
    public static T[] ParseDemographicBreakdownWithPercentage<T>(
        JsonElement response,
        Func<string, double, T> createInstance
    )
    {
        JsonElement.ArrayEnumerator results = response
            .GetProperty("data")[0]
            .GetProperty("total_value")
            .GetProperty("breakdowns")[0]
            .GetProperty("results")
            .EnumerateArray();

        double totalValue = results.Sum(result => result.GetProperty("value").GetDouble());

        var percentages = new List<T>();

        foreach (JsonElement result in results)
        {
            string dimensionValue = result.GetProperty("dimension_values")[0].GetString();
            double value = result.GetProperty("value").GetDouble();
            double percentage = value / totalValue * 100;

            percentages.Add(createInstance(dimensionValue!, percentage));
        }

        return percentages.ToArray();
    }
}
