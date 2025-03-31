using Lanka.Modules.Campaigns.Domain.Bloggers;
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

    public async Task<Pact?> GetByIdAsync(PactId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Pacts.FirstOrDefaultAsync(pact => pact.Id == id, cancellationToken);
    }

    public Task<Pact?> GetByBloggerIdAsync(BloggerId bloggerId, CancellationToken cancellationToken = default)
    {
        return this._dbContext.Pacts.FirstOrDefaultAsync(pact => pact.BloggerId == bloggerId, cancellationToken);
    }

    public async Task<Pact?> GetByIdWithOffersAsync(PactId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Pacts
            .Include(pact => pact.Offers)
            .FirstOrDefaultAsync(pact => pact.Id == id, cancellationToken);
    }

    public async Task<Pact?> GetByBloggerIdWithOffersAsync(BloggerId bloggerId, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Pacts
            .Include(pact => pact.Offers)
            .FirstOrDefaultAsync(pact => pact.BloggerId == bloggerId, cancellationToken);
    }

    public void Add(Pact pact)
    {
        this._dbContext.Pacts.Add(pact);
    }
}
