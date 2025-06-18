using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.Instagram.RefreshToken;

public sealed record RefreshInstagramTokenCommand(Guid UserId, string Code) : ICommand;
