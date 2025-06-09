namespace Lanka.Modules.Analytics.Application.Abstractions.Models;

public class InstagramPeriodRequest
{
    public InstagramPeriodRequest(
        string accessToken,
        string instagramAccountId,
        StatisticsPeriod period
    )
    {
        (DateOnly Since, DateOnly Until) dateRange = ConvertPeriodToDateRange(period);

        this.AccessToken = accessToken;
        this.InstagramAccountId = instagramAccountId;
        this.Since = dateRange.Since;
        this.Until = dateRange.Until;
    }

    public string AccessToken { get; set; }
    public string InstagramAccountId { get; set; }
    public DateOnly Since { get; set; }
    public DateOnly Until { get; set; }

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
