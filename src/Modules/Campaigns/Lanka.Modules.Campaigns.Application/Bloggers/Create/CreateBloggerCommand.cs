using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Create;

public sealed record CreateBloggerCommand(
    Guid BloggerId,
    string Email,
    string FirstName,
    string LastName,
    DateOnly BirthDate
) : ICommand;
