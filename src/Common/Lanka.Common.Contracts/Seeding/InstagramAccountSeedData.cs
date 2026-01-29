namespace Lanka.Common.Contracts.Seeding;

public sealed record InstagramAccountSeedData
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FacebookPageId { get; init; }
    public required string AdvertisementAccountId { get; init; }
    public required string MetadataId { get; init; }
    public required long MetadataIgId { get; init; }
    public required string MetadataUserName { get; init; }
    public required int MetadataFollowersCount { get; init; }
    public required int MetadataMediaCount { get; init; }
    public DateTimeOffset? LastUpdatedAtUtc { get; init; }
}

public sealed record InstagramTokenSeedData
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required Guid InstagramAccountId { get; init; }
    public required string AccessToken { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }
}
