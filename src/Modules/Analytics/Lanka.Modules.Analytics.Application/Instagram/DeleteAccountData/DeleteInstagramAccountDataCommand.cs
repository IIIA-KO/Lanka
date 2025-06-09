using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.Instagram.DeleteAccountData;

public sealed record DeleteInstagramAccountDataCommand(Guid UserId) : ICommand;
