using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.IGAccounts.FBPageIds;

public static class FBPageIdErrors
{
    public static Error Empty => Error.Failure(
        "FBPageId.Empty",
        "FBPageId is empty."
    );
}
