using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.Metadatas;

public static class MetadataErrors
{
    public static Error InsufficientFollowers =>
        Error.Validation(
            "InsufficientFollowers",
            "Instagram account must have at least 100 followers in order to be added."
        );
}
