using System.Globalization;

namespace Lanka.Modules.Analytics.Domain;

public class InstagramPeriodCalculator
{
    public InstagramPeriodCalculator(
        StatisticsPeriod period
    )
    {
        (DateOnly Since, DateOnly Until) dateRange = ConvertPeriodToDateRange(period);

        this.Since = dateRange.Since;
        this.Until = dateRange.Until;
    }

    public DateOnly Since { get; set; }
    public DateOnly Until { get; set; }

    /// <summary>
    /// Returns <see cref="Since"/> as a Unix timestamp string (seconds since epoch).
    /// Instagram Graph API requires Unix timestamps for since/until parameters.
    /// </summary>
    public string SinceUnix => ToUnixTimestamp(this.Since).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns <see cref="Until"/> as a Unix timestamp string (seconds since epoch).
    /// Instagram Graph API requires Unix timestamps for since/until parameters.
    /// </summary>
    public string UntilUnix => ToUnixTimestamp(this.Until).ToString(CultureInfo.InvariantCulture);

    private static long ToUnixTimestamp(DateOnly date)
    {
        return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeSeconds();
    }

    private static (DateOnly Since, DateOnly Until) ConvertPeriodToDateRange(
        StatisticsPeriod period
    )
    {
        return period switch
        {
            StatisticsPeriod.Day21
                => (
                    DateOnly.FromDateTime(DateTime.Today.AddDays(-21)),
                    DateOnly.FromDateTime(DateTime.Today)
                ),
            StatisticsPeriod.Week
                => (
                    DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
                    DateOnly.FromDateTime(DateTime.Today)
                ),
            _
                => (
                    DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                    DateOnly.FromDateTime(DateTime.Today)
                )
        };
    }
}

public enum StatisticsPeriod
{
    Day = 1,
    Week = 7,
    Day21 = 21
}
