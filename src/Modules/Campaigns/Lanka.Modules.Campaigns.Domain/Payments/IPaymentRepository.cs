using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Domain.Payments;

public interface IPaymentRepository
{
    Task<Payment?> GetByCampaignIdAsync(CampaignId campaignId, CancellationToken cancellationToken = default);

    Task<Payment?> GetByOrderIdAsync(string liqPayOrderId, CancellationToken cancellationToken = default);

    void Add(Payment payment);
}
