namespace Lanka.Common.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public OutboxMessage(
        Guid id,
        string type,
        string content,
        DateTime occurredOnUtc
    )
    {
        this.Id = id;
        this.Type = type;
        this.Content = content;
        this.OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; init; }

    public string Type { get; init; }

    public string Content { get; init; }

    public DateTime OccurredOnUtc { get; init; }

    public DateTime? ProcessedOnUtc { get; init; }

    public string? Error { get; init; }
}
