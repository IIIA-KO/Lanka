using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Abstractions.Payments;

namespace Lanka.Modules.Campaigns.Application.Payments.ProcessCallback;

public sealed record ProcessPaymentCallbackCommand(string Payload) : ICommand<PaymentCallbackResponse>;
