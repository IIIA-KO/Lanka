namespace Lanka.Common.Domain;

public interface IEntity
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
