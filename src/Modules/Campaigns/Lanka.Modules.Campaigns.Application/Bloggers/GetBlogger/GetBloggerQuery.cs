using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;

public sealed record GetBloggerQuery(Guid BloggerId) : IQuery<BloggerResponse>;
