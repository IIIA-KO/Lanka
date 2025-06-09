using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.AdvertisementAccountIds;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.FacebookPageIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Analytics.Infrastructure.IgAccounts;

internal sealed class InstagramAccountConfiguration : IEntityTypeConfiguration<InstagramAccount>
{
    public void Configure(EntityTypeBuilder<InstagramAccount> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(igAccount => igAccount.Id);

        builder
            .Property(igAccount => igAccount.Id)
            .HasConversion(id => id.Value, value => new InstagramAccountId(value));

        builder
            .Property(igAccount => igAccount.FacebookPageId)
            .HasConversion(fbPageId => fbPageId.Value, value => FacebookPageId.Create(value).Value)
            .IsRequired();

        builder
            .Property(igAccount => igAccount.AdvertisementAccountId)
            .HasConversion(
                adAccountId => adAccountId.Value,
                value => AdvertisementAccountId.Create(value).Value
            )
            .IsRequired();

        builder
            .Property(igAccount => igAccount.UserId)
            .HasConversion(userId => userId.Value, value => new UserId(value))
            .IsRequired();

        builder.OwnsOne(igAccount => igAccount.Metadata);
    }
}
