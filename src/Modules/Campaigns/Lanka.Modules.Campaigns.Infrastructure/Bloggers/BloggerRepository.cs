using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Infrastructure.Database;

namespace Lanka.Modules.Campaigns.Infrastructure.Bloggers;

internal sealed class BloggerRepository : IBloggerRepository
{
    private readonly CampaignsDbContext _dbContext;

    public BloggerRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public void Add(Blogger blogger)
    {
        this._dbContext.Bloggers.Add(blogger);
    }
}
