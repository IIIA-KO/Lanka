using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.IGAccounts.AdvertisementAccountIds;

public static class AdvertisementAccountIdErrors
{
    public static Error Empty => Error.Failure(
        "AdvertisementAccountId.Empty",
        "AdvertisementAccountId is empty"
    );
}
