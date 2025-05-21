using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Delete;

public sealed record DeleteBloggerCommand(Guid BloggerId) : ICommand;
