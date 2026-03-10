using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Campaigns.GetCampaign;

namespace Lanka.Modules.Campaigns.Application.Campaigns.GetBloggerCampaigns;

public sealed record GetBloggerCampaignsQuery(Guid BloggerId) : IQuery<IReadOnlyList<CampaignResponse>>;
