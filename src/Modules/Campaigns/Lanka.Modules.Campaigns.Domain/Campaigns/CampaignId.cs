using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns
{
    public sealed record CampaignId(Guid Value) : TypedEntityId(Value)
    {
        public static CampaignId New() => new(Guid.NewGuid());
    }
}
