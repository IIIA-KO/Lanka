using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Pacts.DomainEvents
{
    public sealed class PactUpdatedDomainEvent(PactId pactId) : DomainEvent
    {
        public PactId PactId { get; init; } = pactId;
    }
}
