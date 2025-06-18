namespace Lanka.Modules.Analytics.Domain.InstagramAccounts;

public sealed record Metadata(
    string Id,
    long IgId,
    string UserName,
    int FollowersCount,
    int MediaCount
);
