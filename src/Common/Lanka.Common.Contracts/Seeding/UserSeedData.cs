namespace Lanka.Common.Contracts.Seeding;

public sealed record UserSeedData
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required DateOnly BirthDate { get; init; }
    public required string IdentityId { get; init; }
    public DateTimeOffset? InstagramAccountLinkedOnUtc { get; init; }
}

public sealed record UserRoleSeedData
{
    public required Guid UserId { get; init; }
    public required string RoleName { get; init; }
}
