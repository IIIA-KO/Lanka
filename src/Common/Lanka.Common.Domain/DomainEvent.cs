namespace Lanka.Common.Domain;

public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        this.Id = Guid.NewGuid();
        this.OccurredOnUtc = DateTime.UtcNow;
    }

    protected DomainEvent(Guid id, DateTime occurredOnUtc)
    {
        this.Id = id;
        this.OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; init; }

    public DateTime OccurredOnUtc { get; init; }
}
