namespace Lanka.Modules.Campaigns.Domain.Campaigns;

public sealed class Report
{
    public string ContentDelivered { get; private set; }

    public string Approach { get; private set; }

    public string? Notes { get; private set; }

    public IReadOnlyList<string> PostPermalinks { get; private set; }

    public DateTimeOffset SubmittedOnUtc { get; private set; }

    private Report() { }

    public static Report Create(
        string contentDelivered,
        string approach,
        string? notes,
        IEnumerable<string> postPermalinks,
        DateTimeOffset utcNow
    )
    {
        return new Report
        {
            ContentDelivered = contentDelivered,
            Approach = approach,
            Notes = notes,
            PostPermalinks = postPermalinks?.ToList().AsReadOnly() ?? [],
            SubmittedOnUtc = utcNow
        };
    }
}
