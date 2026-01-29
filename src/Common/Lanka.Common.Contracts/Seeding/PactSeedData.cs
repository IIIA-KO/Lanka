namespace Lanka.Common.Contracts.Seeding;

public sealed record PactSeedData
{
    public required Guid Id { get; init; }
    public required Guid BloggerId { get; init; }
    public required string Content { get; init; }
    public required DateTimeOffset LastUpdatedOnUtc { get; init; }
    public required string BloggerFirstName { get; init; }
    public required string BloggerLastName { get; init; }
}
