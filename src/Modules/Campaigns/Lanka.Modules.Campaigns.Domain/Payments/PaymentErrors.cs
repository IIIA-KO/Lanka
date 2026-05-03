using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Payment.NotFound", "Payment with specified identifier was not found");

    public static readonly Error NotPending =
        Error.Problem("Payment.NotPending", "Payment is not in Pending status");

    public static readonly Error AlreadyExists =
        Error.Conflict("Payment.AlreadyExists", "A payment already exists for this campaign");

    public static readonly Error InvalidSignature =
        Error.Problem("Payment.InvalidSignature", "LiqPay callback signature is invalid");

    public static readonly Error CampaignNotConfirmed =
        Error.Problem("Payment.CampaignNotConfirmed", "Payment can only be initiated for a Confirmed campaign");
}
