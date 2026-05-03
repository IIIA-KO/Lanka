using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Payments.GetPayment;

public sealed record GetPaymentQuery(Guid CampaignId) : IQuery<PaymentResponse>;
