using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;

public sealed class BloggerUpdatedDomainEvent(Guid bloggerId) : DomainEvent
{
    public Guid BloggerId { get; init; } = bloggerId;
}
