using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Update;

public sealed record UpdateBloggerCommand(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Bio
) : ICommand<BloggerResponse>;
