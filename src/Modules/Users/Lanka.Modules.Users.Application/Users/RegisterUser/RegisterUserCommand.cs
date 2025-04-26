using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Email, 
    string Password, 
    string FirstName, 
    string LastName,
    DateOnly BirthDate
) : ICommand<Guid>;
