namespace Lanka.Modules.Campaigns.Application.Campaigns.SeedCampaigns;

public sealed record SeedCampaignsResult(int Seeded, int Bloggers, IReadOnlyList<string> Months);
