using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Payments;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Payments;

internal sealed class PaymentRepository : IPaymentRepository
{
    private readonly CampaignsDbContext _context;

    public PaymentRepository(CampaignsDbContext context)
    {
        this._context = context;
    }

    public Task<Payment?> GetByCampaignIdAsync(CampaignId campaignId, CancellationToken cancellationToken = default)
    {
        return this._context.Payments
            .SingleOrDefaultAsync(p => p.CampaignId == campaignId, cancellationToken);
    }

    public Task<Payment?> GetByOrderIdAsync(string liqPayOrderId, CancellationToken cancellationToken = default)
    {
        return this._context.Payments
            .SingleOrDefaultAsync(p => p.LiqPayOrderId == liqPayOrderId, cancellationToken);
    }

    public void Add(Payment payment)
    {
        this._context.Payments.Add(payment);
    }
}
