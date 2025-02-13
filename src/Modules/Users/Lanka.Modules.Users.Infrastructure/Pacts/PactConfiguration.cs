using Lanka.Modules.Users.Domain.Pacts;
using Lanka.Modules.Users.Domain.Pacts.Contents;
using Lanka.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Users.Infrastructure.Pacts
{
    public class PactConfiguration : IEntityTypeConfiguration<Pact>
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
                .Property(pact => pact.UserId)
                .HasConversion(userId => userId.Value, value => new UserId(value))
                .IsRequired();

            builder
                .HasMany(pact => pact.Offers)
                .WithOne(offer => offer.Pact)
                .HasForeignKey(offer => offer.PactId)
                .IsRequired();
        }
    }
}
