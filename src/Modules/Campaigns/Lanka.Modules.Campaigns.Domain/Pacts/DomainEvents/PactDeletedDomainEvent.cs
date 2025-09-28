using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.Pacts.DomainEvents;

public sealed class PactDeletedDomainEvent(PactId pactId, BloggerId bloggerId) : DomainEvent
{
    public PactId PactId { get; init; } = pactId;
    public BloggerId BloggerId { get; init; } = bloggerId;
}

