using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;

public record FetchInstagramAccountDataCommand(Guid UserId, string Code) : ICommand;
