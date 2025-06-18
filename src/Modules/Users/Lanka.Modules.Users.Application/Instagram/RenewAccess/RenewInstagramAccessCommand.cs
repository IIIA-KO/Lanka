using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Instagram.RenewAccess;

public sealed record RenewInstagramAccessCommand(string Code) : ICommand;
