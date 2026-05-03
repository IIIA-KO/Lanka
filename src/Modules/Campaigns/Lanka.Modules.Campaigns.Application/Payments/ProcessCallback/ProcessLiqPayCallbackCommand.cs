using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Payments.ProcessCallback;

public sealed record ProcessLiqPayCallbackCommand(string Data, string Signature) : ICommand;
