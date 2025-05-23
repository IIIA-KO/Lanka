using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Application.Users.GetUser;

namespace Lanka.Modules.Users.Application.Users.Update;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateOnly BirthDate
) : ICommand<UserResponse>;
