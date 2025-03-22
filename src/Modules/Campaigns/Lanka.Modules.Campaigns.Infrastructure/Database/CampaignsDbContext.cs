using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.BlockedDates;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Database;

public class CampaignsDbContext(DbContextOptions<CampaignsDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Blogger> Bloggers { get; set; }
        
    internal DbSet<Campaign> Campaigns { get; set; }
        
    internal DbSet<Offer> Offers { get; set; }

    internal DbSet<Pact> Pacts { get; set; }
    
    internal DbSet<BlockedDate> BlockedDates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Campaigns);
            
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampaignsDbContext).Assembly);
            
        base.OnModelCreating(modelBuilder);
    }
}
