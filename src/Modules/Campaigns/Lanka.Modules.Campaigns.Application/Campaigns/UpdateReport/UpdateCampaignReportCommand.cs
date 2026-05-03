using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.UpdateReport;

public sealed record UpdateCampaignReportCommand(
    Guid CampaignId,
    string ContentDelivered,
    string Approach,
    string? Notes,
    List<string> PostPermalinks
) : ICommand;
