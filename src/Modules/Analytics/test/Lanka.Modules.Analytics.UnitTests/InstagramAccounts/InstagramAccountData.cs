namespace Lanka.Modules.Analytics.UnitTests.InstagramAccounts;

internal static class InstagramAccountData
{
    public static Guid UserId => Guid.NewGuid();

    public static string Email => "test@lanka.com";

    public static string FacebookPageId => "facebook_page_id_1234567890";

    public static string AdvertisementAccountId => "advertisement_account_id_1234567890";
    
    public static string BusinessDiscoveryId => "business_discovery_id_1234567890";
    
    public static long BusinessDiscoveryIgId => 123456780;
    
    public static string BusinessDiscoveryUsername => "business_discovery_username";
    
    public static int BusinessDiscoveryFollowersCount => 1000;
    
    public static int BusinessDiscoveryMediaCount => 200;
}
