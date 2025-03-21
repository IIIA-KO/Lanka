using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;

public sealed record GetCampaignQuery(Guid CampaignId) : IQuery<CampaignResponse>;
