using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Instagram.Link;

public sealed record LinkInstagramAccountCommand(string Code) : ICommand;
