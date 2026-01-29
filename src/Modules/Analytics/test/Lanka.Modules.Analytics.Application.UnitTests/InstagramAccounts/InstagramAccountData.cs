using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Metadatas;

namespace Lanka.Modules.Analytics.Application.UnitTests.InstagramAccounts;

internal static class InstagramAccountData
{
    public static InstagramAccount Create()
    {
        return InstagramAccount.Create(
            UserId,
            Email,
            FacebookPageId,
            AdvertisementAccountId,
            BusinessDiscoveryId,
            BusinessDiscoveryIgId,
            BusinessDiscoveryUsername,
            BusinessDiscoveryFollowersCount,
            BusinessDiscoveryMediaCount
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

    public static string Email => "test@lanka.com";

    public static string FacebookPageId => "facebook_page_id_1234567890";

    public static string AdvertisementAccountId => "advertisement_account_id_1234567890";

    public static string BusinessDiscoveryId => "business_discovery_id_1234567890";
    
    public static long BusinessDiscoveryIgId => 123456780;
    
    public static string BusinessDiscoveryUsername => "business_discovery_username";
    
    public static int BusinessDiscoveryFollowersCount => 1000;
    
    public static int BusinessDiscoveryMediaCount => 200;
    
    public static Metadata Metadata => Metadata.Create(
        BusinessDiscoveryId,
        BusinessDiscoveryIgId,
        BusinessDiscoveryUsername,
        BusinessDiscoveryFollowersCount,
        BusinessDiscoveryMediaCount
    ).Value;
}
