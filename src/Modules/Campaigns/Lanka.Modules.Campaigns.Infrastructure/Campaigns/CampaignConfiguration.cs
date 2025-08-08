using Lanka.Common.Contracts.Currencies;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Campaigns.Descriptions;
using Lanka.Modules.Campaigns.Domain.Campaigns.Names;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Campaigns;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");

        builder.HasKey(campaign => campaign.Id);

        builder
            .Property(campaign => campaign.Id)
            .HasConversion(id => id.Value, value => new CampaignId(value))
            .IsRequired();

        builder
            .Property(campaign => campaign.Name)
            .HasMaxLength(Name.MaxLength)
            .HasConversion(name => name.Value, value => Name.Create(value).Value)
            .IsRequired();

        builder
            .Property(campaign => campaign.Description)
            .HasMaxLength(Description.MaxLength)
            .HasConversion(description => description.Value, value => Description.Create(value).Value)
            .IsRequired();

        builder
            .OwnsOne(
                offer => offer.Price,
                priceBuilder =>
                {
                    priceBuilder
                        .Property(money => money.Amount)
                        .IsRequired();

                    priceBuilder
                        .Property(money => money.Currency)
                        .HasConversion(currency => currency.Code.ToString(), code => Currency.FromCode(code))
                        .IsRequired();
                }
            );

        builder
            .HasOne(campaign => campaign.Offer)
            .WithMany()
            .HasForeignKey(campaign => campaign.OfferId)
            .IsRequired();

        builder
            .HasOne(campaign => campaign.Client)
            .WithMany()
            .HasForeignKey(campaign => campaign.ClientId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(campaign => campaign.Creator)
            .WithMany()
            .HasForeignKey(campaign => campaign.CreatorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
