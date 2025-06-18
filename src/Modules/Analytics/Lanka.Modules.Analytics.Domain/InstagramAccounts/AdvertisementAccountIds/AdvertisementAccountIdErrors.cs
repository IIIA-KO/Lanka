using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.AdvertisementAccountIds;

public static class AdvertisementAccountIdErrors
{
    public static Error Empty => Error.Failure(
        "AdvertisementAccountId.Empty",
        "AdvertisementAccountId is empty"
    );
}
