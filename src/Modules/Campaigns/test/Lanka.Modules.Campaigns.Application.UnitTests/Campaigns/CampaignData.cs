using Lanka.Modules.Campaigns.Application.UnitTests.Offers;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Campaigns;

internal static class CampaignData
{
    public static string Name => "Test Campaign";

    public static string Description => "Test Description";
    
    public static DateTimeOffset ScheduledOnUtc => DateTimeOffset.UtcNow.AddDays(7);

    public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    
    public static Campaign CreatePendingCampaign()
    {
        return Campaign
            .Pend(
                Name,
                Description,
                DateTimeOffset.UtcNow.AddDays(7),
                OfferData.CreateOffer(),
                BloggerId.New(),
                BloggerId.New(),
                DateTimeOffset.UtcNow
            ).Value;
    }

    public static Campaign CreateConfirmedCampaign()
    {
        Campaign campaign = CreatePendingCampaign();
        campaign.Confirm(DateTimeOffset.UtcNow);
        return campaign;
    }

    public static Campaign CreateDoneCampaign()
    {
        Campaign campaign = CreateConfirmedCampaign();
        campaign.MarkAsDone(DateTimeOffset.UtcNow);
        return campaign;
    }

    public static Campaign CreateCompletedCampaign()
    {
        Campaign campaign = CreateDoneCampaign();
        campaign.Complete(DateTimeOffset.UtcNow);
        return campaign;
    }
}
