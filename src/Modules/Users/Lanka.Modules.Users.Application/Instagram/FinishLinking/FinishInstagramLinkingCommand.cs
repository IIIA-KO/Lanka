using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Instagram.FinishLinking;

public sealed record FinishInstagramLinkingCommand(Guid UserId, string Username, string IgId) : ICommand;
