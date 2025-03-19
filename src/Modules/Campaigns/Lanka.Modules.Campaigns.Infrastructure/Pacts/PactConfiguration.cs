using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Lanka.Modules.Campaigns.Domain.Pacts.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Pacts
{
    internal sealed class PactConfiguration : IEntityTypeConfiguration<Pact>
    {
        public void Configure(EntityTypeBuilder<Pact> builder)
        {
            builder.ToTable("pacts");
            
            builder.HasKey(pact => pact.Id);

            builder
                .Property(pact => pact.Id)
                .HasConversion(id => id.Value, value => new PactId(value));
            
            builder
                .Property(pact => pact.Content)
                .HasConversion(content => content.Value, value => Content.Create(value).Value)
                .IsRequired();

            builder
                .Property(pact => pact.BloggerId)
                .HasConversion(userId => userId.Value, value => new BloggerId(value))
                .IsRequired();

            builder
                .HasMany(pact => pact.Offers)
                .WithOne(offer => offer.Pact)
                .HasForeignKey(offer => offer.PactId)
                .IsRequired();
        }
    }
}
