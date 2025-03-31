using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Users.Login;

public sealed record LoginUserCommand(string Email, string Password)
    : ICommand<AccessTokenResponse>;
