using Lanka.Modules.Analytics.Domain.IGAccounts;
using Lanka.Modules.Analytics.Domain.IGAccounts.AdvertisementAccountIds;
using Lanka.Modules.Analytics.Domain.IGAccounts.FBPageIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Analytics.Infrastructure.IGAccounts;

internal sealed class IGAccountConfiguration : IEntityTypeConfiguration<IGAccount>
{
    public void Configure(EntityTypeBuilder<IGAccount> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(igAccount => igAccount.Id);

        builder
            .Property(igAccount => igAccount.Id)
            .HasConversion(id => id.Value, value => new IGAccountId(value));

        builder
            .Property(igAccount => igAccount.FBPageId)
            .HasConversion(fbPageId => fbPageId.Value, value => FBPageId.Create(value).Value)
            .IsRequired();

        builder
            .Property(igAccount => igAccount.AdvertisementAccountId)
            .HasConversion(
                adAccountId => adAccountId.Value,
                value => AdvertisementAccountId.Create(value).Value)
            .IsRequired();

        builder
            .Property(igAccount => igAccount.BloggerId)
            .HasConversion(bloggerId => bloggerId.Value, value => new BloggerId(value))
            .IsRequired();

        builder.OwnsOne(igAccount => igAccount.Metadata);
    }
}
