using Lanka.Common.Contracts.Currencies;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Offers.Descriptions;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Name = Lanka.Modules.Campaigns.Domain.Offers.Names.Name;

namespace Lanka.Modules.Campaigns.Infrastructure.Offers
{
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.ToTable("offers");
            
            builder.HasKey(x => x.Id);

            builder
                .Property(offer => offer.Id)
                .HasConversion(id => id.Value, value => new OfferId(value));

            builder
                .Property(offer => offer.Name)
                .HasMaxLength(Name.MaxLength)
                .HasConversion(name => name.Value, value => Name.Create(value).Value)
                .IsRequired();
            
            builder
                .Property(offer => offer.Description)
                .HasMaxLength(Description.MaxLength)
                .HasConversion(description => description.Value, value => Description.Create(value).Value)
                .IsRequired();
            
            builder
                .Property(offer => offer.PactId)
                .HasConversion(pactId => pactId.Value, value => new PactId(value));

            builder
                .HasOne(offer => offer.Pact)
                .WithMany(pact => pact.Offers)
                .HasForeignKey(offer => offer.PactId)
                .IsRequired();

            builder
                .OwnsOne(
                    offer => offer.Price,
                    priceBuilder =>
                        priceBuilder
                            .Property(money => money.Currency)
                            .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                            .IsRequired()
                );

            builder.Property<uint>("Version").IsRowVersion();
        }
    }
}
