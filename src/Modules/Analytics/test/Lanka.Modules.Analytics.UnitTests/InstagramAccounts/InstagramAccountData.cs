using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.UnitTests.InstagramAccounts;

internal static class InstagramAccountData
{
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
