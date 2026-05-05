using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Payments.Initiate;

public sealed record InitiatePaymentCommand(Guid CampaignId) : ICommand<PaymentCheckoutResponse>;
