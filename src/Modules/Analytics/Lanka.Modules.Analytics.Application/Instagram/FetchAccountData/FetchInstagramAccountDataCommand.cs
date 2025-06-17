using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;

public sealed record FetchInstagramAccountDataCommand(Guid UserId, string Code) : ICommand;
