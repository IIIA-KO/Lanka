using System.Text.Json;

namespace Lanka.Modules.Analytics.Domain;

internal static class InstagramJsonService
{
    public static int ParseMetricTotalValue(JsonElement response, string metric)
    {
        foreach (JsonElement element in response.GetProperty("data").EnumerateArray())
        {
            if (element.TryGetProperty("name", out JsonElement nameProp)
                && nameProp.GetString() == metric
                && element.TryGetProperty("total_value", out JsonElement totalValue)
                && totalValue.TryGetProperty("value", out JsonElement value))
            {
                return value.GetInt32();
            }
        }

        return 0;
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
