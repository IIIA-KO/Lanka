using Lanka.Common.Contracts.Monies;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Domain.Payments;

public sealed class Payment : Entity<PaymentId>
{
    public CampaignId CampaignId { get; private set; }

    public BloggerId ClientId { get; private set; }

    public Money Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string LiqPayOrderId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    private Payment() { }

    public static Payment Create(
        CampaignId campaignId,
        BloggerId clientId,
        Money amount,
        string liqPayOrderId,
        DateTimeOffset utcNow
    )
    {
        return new Payment
        {
            Id = PaymentId.New(),
            CampaignId = campaignId,
            ClientId = clientId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            LiqPayOrderId = liqPayOrderId,
            CreatedAtUtc = utcNow
        };
    }

    public Result Complete(DateTimeOffset utcNow)
    {
        if (this.Status != PaymentStatus.Pending)
        {
            return Result.Failure(PaymentErrors.NotPending);
        }

        this.Status = PaymentStatus.Completed;
        this.PaidAtUtc = utcNow;

        return Result.Success();
    }

    public Result Fail()
    {
        if (this.Status != PaymentStatus.Pending)
        {
            return Result.Failure(PaymentErrors.NotPending);
        }

        this.Status = PaymentStatus.Failed;

        return Result.Success();
    }
}
