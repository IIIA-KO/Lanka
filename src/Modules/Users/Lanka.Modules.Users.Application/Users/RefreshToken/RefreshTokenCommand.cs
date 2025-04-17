using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Application.Users.Login;

namespace Lanka.Modules.Users.Application.Users.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AccessTokenResponse>;
