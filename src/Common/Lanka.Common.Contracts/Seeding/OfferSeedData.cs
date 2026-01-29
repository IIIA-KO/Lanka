namespace Lanka.Common.Contracts.Seeding;

public sealed record OfferSeedData
{
    public required Guid Id { get; init; }
    public required Guid PactId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required decimal PriceAmount { get; init; }
    public required string PriceCurrency { get; init; }
    public DateTimeOffset? LastCooperatedOnUtc { get; init; }
    public required Guid BloggerId { get; init; }
    public required string BloggerFirstName { get; init; }
    public required string BloggerLastName { get; init; }
}
