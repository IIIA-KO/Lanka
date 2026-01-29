namespace Lanka.Common.Contracts.Seeding;

public sealed record BloggerSeedData
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required DateOnly BirthDate { get; init; }
    public required string Bio { get; init; }
    public required string CategoryName { get; init; }
    public string? InstagramMetadataUsername { get; init; }
    public int? InstagramMetadataFollowersCount { get; init; }
    public int? InstagramMetadataMediaCount { get; init; }
    public string? ProfilePhotoId { get; init; }
    public string? ProfilePhotoUri { get; init; }
}
