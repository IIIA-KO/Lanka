using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.FacebookPageIds;

public static class FacebookPageIdErrors
{
    public static Error Empty => Error.Failure(
        "FacebookPageId.Empty",
        "FacebookPageId is empty."
    );
}
