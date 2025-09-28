using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;

public sealed class BloggerDeletedDomainEvent(BloggerId bloggerId) : DomainEvent
{
    public BloggerId BloggerId { get; init; } = bloggerId;
}
