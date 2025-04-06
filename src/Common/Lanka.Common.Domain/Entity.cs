namespace Lanka.Common.Domain;

public abstract class Entity<TEntityId> : IEntity
    where TEntityId : TypedEntityId
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TEntityId Id { get; protected init; }

    protected Entity() { }

    protected Entity(TEntityId id)
    {
        this.Id = id;
    }

    public IReadOnlyList<IDomainEvent> DomainEvents => this._domainEvents;

    public void ClearDomainEvents()
    {
        this._domainEvents.Clear();
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        this._domainEvents.Add(domainEvent);
    }
}
