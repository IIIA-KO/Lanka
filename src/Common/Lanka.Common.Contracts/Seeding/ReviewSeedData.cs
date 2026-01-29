namespace Lanka.Common.Contracts.Seeding;

public sealed record ReviewSeedData
{
    public required Guid Id { get; init; }
    public required Guid CampaignId { get; init; }
    public required Guid ClientId { get; init; }
    public required Guid CreatorId { get; init; }
    public required Guid OfferId { get; init; }
    public required int Rating { get; init; }
    public required string Comment { get; init; }
    public required DateTimeOffset CreatedOnUtc { get; init; }
    public required string CampaignName { get; init; }
}
