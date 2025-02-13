using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns
{
    public class Campaign : Entity<CampaignId>
    {
        public static readonly CampaignStatus[] activeCampaignStatuses =
        [
            CampaignStatus.Pending,
            CampaignStatus.Confirmed,
            CampaignStatus.Done
        ];
        
        private Campaign() {}
        
        
    }
}
