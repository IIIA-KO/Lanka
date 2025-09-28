using Lanka.Common.Infrastructure.Inbox;
using Lanka.Common.Infrastructure.Outbox;
using Lanka.Modules.Matching.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Matching.Infrastructure.Database;

public sealed class MatchingDbContext : DbContext, IUnitOfWork
{
    public MatchingDbContext(DbContextOptions<MatchingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Matching);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
