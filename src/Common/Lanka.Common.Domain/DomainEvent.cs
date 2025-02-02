namespace Lanka.Common.Domain
{
    public abstract class DomainEvent : IDomainEvent
    {
        protected DomainEvent()
        {
            this.Id = Guid.NewGuid();
            this.OcurredOnUtc = DateTime.UtcNow;
        }

        protected DomainEvent(Guid id, DateTime occurredOnUtc)
        {
            this.Id = id;
            this.OcurredOnUtc = occurredOnUtc;
        }

        public Guid Id { get; init; }

        public DateTime OcurredOnUtc { get; init; }
    }
}
