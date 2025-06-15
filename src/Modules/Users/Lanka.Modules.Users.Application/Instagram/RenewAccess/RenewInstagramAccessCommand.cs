using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Instagram.RenewAccess;

public record RenewInstagramAccessCommand(string Code) : ICommand;
