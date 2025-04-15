using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Bloggers.UpdateBlogger;

public sealed record UpdateBloggerCommand(
    Guid BloggerId,
    string FirstName,
    string LastName,
    DateOnly BirthDate
) : ICommand;
