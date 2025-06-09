using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Instagram.Link;

public record LinkInstagramAccountCommand(string Code) : ICommand;
