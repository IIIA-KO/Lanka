using Lanka.Common.Infrastructure.Inbox;
using Lanka.Common.Infrastructure.Outbox;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.Infrastructure.Encryption;
using Lanka.Modules.Analytics.Infrastructure.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Analytics.Infrastructure.Database;

public class AnalyticsDbContext(
    DbContextOptions<AnalyticsDbContext> options,
    EncryptionService encryptionService
) : DbContext(options), IUnitOfWork
{
    internal DbSet<InstagramAccount> InstagramAccounts { get; set; }

    internal DbSet<Token> Tokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Analytics);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());

        modelBuilder.ApplyConfiguration(new TokenConfiguration(encryptionService));

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
