using Lanka.Modules.Campaigns.Domain.Pacts;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Pacts;

internal sealed class PactRepository : IPactRepository
{
    private readonly CampaignsDbContext _dbContext;

    public PactRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Pact?> GetByIdWithOffersAsync(PactId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Pacts
            .Include(pact => pact.Offers)
            .FirstOrDefaultAsync(pact => pact.Id == id, cancellationToken);
    }
}
