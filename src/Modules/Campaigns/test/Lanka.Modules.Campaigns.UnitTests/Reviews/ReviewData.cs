using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Reviews.Comments;
using Lanka.Modules.Campaigns.Domain.Reviews.Ratings;
using Lanka.Modules.Campaigns.UnitTests.Campaigns;

namespace Lanka.Modules.Campaigns.UnitTests.Reviews;

internal static class ReviewData
{
    public static int ValidRating => 5;

    public static string ValidComment => "This is a valid comment";
    public static string InvalidComment => string.Empty;

    public static Campaign CompletedCooperation =>
        CampaignData.CreateCompletedCampaign();

    public static Campaign NotCompletedCooperation =>
        CampaignData.CreatePendingCampaign();

    public static DateTimeOffset CreatedOnUtc => DateTimeOffset.UtcNow;
}
