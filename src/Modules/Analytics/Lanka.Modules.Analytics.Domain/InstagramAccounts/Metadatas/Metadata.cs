using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.Metadatas;

public sealed record Metadata
{
    public const int MinFollowers = 100;

    public string Id { get; init; }
    public long IgId { get; init; }
    public string UserName { get; init; }
    public int FollowersCount { get; init; }
    public int MediaCount { get; init; }
    
    private Metadata(
        string id,
        long igId,
        string userName,
        int followersCount,
        int mediaCount
    )
    {
        this.Id = id;
        this.IgId = igId;
        this.UserName = userName;
        this.FollowersCount = followersCount;
        this.MediaCount = mediaCount;
    }
    
    public static Result<Metadata> Create(
        string id,
        long igId,
        string userName,
        int followersCount,
        int mediaCount
    )
    {
        if (followersCount < MinFollowers)
        {
            return Result.Failure<Metadata>(MetadataErrors.InsufficientFollowers);
        }

        return new Metadata(id, igId, userName, followersCount, mediaCount);
    }
}
