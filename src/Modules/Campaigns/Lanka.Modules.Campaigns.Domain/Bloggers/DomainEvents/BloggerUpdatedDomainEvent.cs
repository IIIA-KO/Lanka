using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;

public sealed class BloggerUpdatedDomainEvent(BloggerId bloggerId) : DomainEvent
{
    public BloggerId BloggerId { get; init; } = bloggerId;
}
