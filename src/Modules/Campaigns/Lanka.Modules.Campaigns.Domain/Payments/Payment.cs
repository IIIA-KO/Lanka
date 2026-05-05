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

    public string ProviderOrderId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    private Payment() { }

    public static Payment Create(
        CampaignId campaignId,
        BloggerId clientId,
        Money amount,
        string providerOrderId,
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
            ProviderOrderId = providerOrderId,
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

    public Result RefreshProviderOrderId(string providerOrderId)
    {
        if (this.Status != PaymentStatus.Pending)
        {
            return Result.Failure(PaymentErrors.NotPending);
        }

        this.ProviderOrderId = providerOrderId;

        return Result.Success();
    }

    public Result Retry(string providerOrderId)
    {
        if (this.Status != PaymentStatus.Failed)
        {
            return Result.Failure(PaymentErrors.NotFailed);
        }

        this.Status = PaymentStatus.Pending;
        this.ProviderOrderId = providerOrderId;
        this.PaidAtUtc = null;

        return Result.Success();
    }
}
