using Lanka.Modules.Campaigns.Application.UnitTests.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Reviews;

internal static class ReviewData
{
    public static Review CreateReview()
    {
        return Review.Create(
            CampaignData.CreateCompletedCampaign(),
            ValidRating,
            ValidComment,
            CreatedOnUtc
        ).Value;
    }
    
    public static int ValidRating => 5;

    public static string ValidComment => "This is a valid comment";
    public static string InvalidComment => string.Empty;

    public static Campaign CompletedCooperation =>
        CampaignData.CreateCompletedCampaign();

    public static Campaign NotCompletedCooperation =>
        CampaignData.CreatePendingCampaign();

    public static DateTimeOffset CreatedOnUtc => DateTimeOffset.UtcNow;
}
