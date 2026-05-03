using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.GetCampaignReport;

public sealed record GetCampaignReportQuery(Guid CampaignId) : IQuery<CampaignReportResponse>;
