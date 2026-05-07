using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Bloggers.UpdatePayoutAccount;

public sealed record UpdatePayoutAccountCommand(string Iban, string Currency) : ICommand;
