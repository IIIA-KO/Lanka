namespace Lanka.Common.Contracts.Seeding;

public sealed record CampaignSeedData
{
    public required Guid Id { get; init; }
    public required Guid CreatorId { get; init; }
    public required Guid ClientId { get; init; }
    public required Guid OfferId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset ScheduledOnUtc { get; init; }
    public required int Status { get; init; }
    public required DateTimeOffset PendedOnUtc { get; init; }
    public DateTimeOffset? ConfirmedOnUtc { get; init; }
    public DateTimeOffset? RejectedOnUtc { get; init; }
    public DateTimeOffset? DoneOnUtc { get; init; }
    public DateTimeOffset? CompletedOnUtc { get; init; }
    public DateTimeOffset? CancelledOnUtc { get; init; }
    public required decimal PriceAmount { get; init; }
    public required string PriceCurrency { get; init; }
    public required string CreatorFirstName { get; init; }
    public required string CreatorLastName { get; init; }
    public required string ClientFirstName { get; init; }
    public required string ClientLastName { get; init; }
    public string? ReportContentDelivered { get; init; }
    public string? ReportApproach { get; init; }
    public string? ReportNotes { get; init; }
    public string[]? ReportPostPermalinks { get; init; }
    public DateTimeOffset? ReportSubmittedOnUtc { get; init; }
}
