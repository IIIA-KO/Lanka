using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Instagram.FinishInstagramLinking;

public record FinishInstagramLinkingCommand(Guid UserId, string Username, string IgId) : ICommand;
