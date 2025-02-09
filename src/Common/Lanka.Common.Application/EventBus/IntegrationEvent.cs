namespace Lanka.Common.Application.EventBus
{
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        protected IntegrationEvent(Guid id, DateTime occurredOnUtc)
        {
            this.Id = id;
            this.OccurredOnUtc = occurredOnUtc;
        }

        public Guid Id { get; init; }
        
        public DateTime OccurredOnUtc { get; init; }
    }
}
