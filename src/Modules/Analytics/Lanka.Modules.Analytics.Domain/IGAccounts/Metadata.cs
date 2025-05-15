using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.IGAccounts;

public sealed record Metadata(
    string id,
    long igId,
    string userName,
    int followersCount,
    int mediaCount
);
