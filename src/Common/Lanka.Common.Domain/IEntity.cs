namespace Lanka.Common.Domain
{
    public interface IEntity
    {
        IReadOnlyList<IDomainEvent> GetDomainEvents();
        
        void ClearDomainEvents();
    }
}
