using Lanka.Common.Contracts.Currencies;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Payments;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder
            .Property(p => p.Id)
            .HasConversion(id => id.Value, value => new PaymentId(value))
            .IsRequired();

        builder
            .Property(p => p.CampaignId)
            .HasConversion(id => id.Value, value => new CampaignId(value))
            .IsRequired();

        builder
            .Property(p => p.ClientId)
            .HasConversion(id => id.Value, value => new BloggerId(value))
            .IsRequired();

        builder.OwnsOne(
            p => p.Amount,
            amountBuilder =>
            {
                amountBuilder
                    .Property(m => m.Amount)
                    .IsRequired();

                amountBuilder
                    .Property(m => m.Currency)
                    .HasConversion(c => c.Code.ToString(), code => Currency.FromCode(code))
                    .IsRequired();
            }
        );

        builder
            .Property(p => p.Status)
            .IsRequired();

        builder
            .Property(p => p.LiqPayOrderId)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder
            .HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(p => p.CampaignId)
            .IsRequired();

        builder
            .HasIndex(p => p.CampaignId)
            .IsUnique();

        builder
            .HasIndex(p => p.LiqPayOrderId)
            .IsUnique();
    }
}
