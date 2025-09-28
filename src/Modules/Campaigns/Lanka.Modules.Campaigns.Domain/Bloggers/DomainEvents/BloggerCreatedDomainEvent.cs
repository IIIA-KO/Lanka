using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;

public sealed class BloggerCreatedDomainEvent(BloggerId bloggerId) : DomainEvent
{
    public BloggerId BloggerId { get; init; } = bloggerId;
}
