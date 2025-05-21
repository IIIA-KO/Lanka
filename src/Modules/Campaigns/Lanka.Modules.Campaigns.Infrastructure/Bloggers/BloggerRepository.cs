using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Bloggers;

internal sealed class BloggerRepository : IBloggerRepository
{
    private readonly CampaignsDbContext _dbContext;

    public BloggerRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Blogger?> GetByIdAsync(
        BloggerId id,
        CancellationToken cancellationToken = default
    )
    {
        return await this._dbContext.Bloggers
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Add(Blogger blogger)
    {
        this._dbContext.Bloggers.Add(blogger);
    }
    
    public void Remove(Blogger blogger)
    {
        this._dbContext.Bloggers.Remove(blogger);
    }
}
