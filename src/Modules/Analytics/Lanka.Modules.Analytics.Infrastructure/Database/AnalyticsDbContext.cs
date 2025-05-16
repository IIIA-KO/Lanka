using Lanka.Modules.Analytics.Domain.IGAccounts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Analytics.Infrastructure.Database;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<IGAccount> IGAccounts { get; set; }
    
    internal DbSet<Token> Tokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Analytics);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
