using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.UnitTests.InstagramAccounts;

internal static class InstagramAccountData
{
    public static InstagramAccount Create()
    {
        return InstagramAccount.Create(
            UserId,
            FacebookPageId,
            AdvertisementAccountId,
            Metadata
        ).Value;
    }

    public static InstagramUserInfo InstagramUserInfo => new()
    {
        BusinessDiscovery = new BusinessDiscovery
        {
            Username = Metadata.UserName,
            Name = "Test User",
            IgId = Metadata.IgId,
            Id = Metadata.Id,
            FollowersCount = Metadata.FollowersCount,
            MediaCount = Metadata.MediaCount,
        },
        Id = Metadata.Id,
        FacebookPageId = FacebookPageId,
        AdAccountId = AdvertisementAccountId,
    };

    public static Guid UserId => Guid.NewGuid();

    public static string FacebookPageId => "facebook_page_id_1234567890";

    public static string AdvertisementAccountId => "advertisement_account_id_1234567890";

    public static Metadata Metadata => new(
        "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        1234567890,
        "username123",
        100,
        100
    );
}
