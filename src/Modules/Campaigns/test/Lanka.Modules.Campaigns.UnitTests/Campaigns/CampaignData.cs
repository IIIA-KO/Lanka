using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns.Descriptions;
using Lanka.Modules.Campaigns.Domain.Campaigns.Names;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;
using Lanka.Modules.Campaigns.UnitTests.Offers;

namespace Lanka.Modules.Campaigns.UnitTests.Campaigns;

internal static class CampaignData
{
    public static string Name => BaseTest.Faker.Company.CompanyName();

    public static string Description => BaseTest.Faker.Lorem.Sentence();

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
