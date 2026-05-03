namespace Lanka.Modules.Campaigns.Application.Campaigns.GetCampaignReport;

public sealed record CampaignReportResponse(
    string ContentDelivered,
    string Approach,
    string? Notes,
    List<string> PostPermalinks,
    DateTimeOffset SubmittedOnUtc
);
