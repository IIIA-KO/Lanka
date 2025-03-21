using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Bloggers;

public class BloggerConfiguration : IEntityTypeConfiguration<Blogger>
{
    public void Configure(EntityTypeBuilder<Blogger> builder)
    {
        builder.ToTable("bloggers");
            
        builder.HasKey(blogger => blogger.Id);
            
        builder
            .HasIndex(blogger => blogger.Email)
            .IsUnique();
            
        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new BloggerId(value));
            
        builder
            .Property(blogger => blogger.FirstName)
            .HasConversion(firstName => firstName.Value, value => new FirstName(value))
            .IsRequired();
            
        builder
            .Property(blogger => blogger.LastName)
            .HasConversion(lastName => lastName.Value, value => new LastName(value))
            .IsRequired();
            
        builder
            .Property(blogger => blogger.Email)
            .HasConversion(email => email.Value, value => new Email(value))
            .IsRequired();
            
        builder
            .Property(blogger => blogger.BirthDate)
            .HasConversion(birthDate => birthDate.Value, value => new BirthDate(value))
            .IsRequired();
            
        builder
            .HasOne(blogger => blogger.Pact)
            .WithOne(pact => pact.Blogger)
            .HasForeignKey<Pact>(pact => pact.BloggerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
