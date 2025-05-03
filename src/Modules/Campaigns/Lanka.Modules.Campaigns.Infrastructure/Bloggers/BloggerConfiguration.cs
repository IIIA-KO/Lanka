using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.Bios;
using Lanka.Modules.Campaigns.Domain.Bloggers.BirthDates;
using Lanka.Modules.Campaigns.Domain.Bloggers.Emails;
using Lanka.Modules.Campaigns.Domain.Bloggers.FirstNames;
using Lanka.Modules.Campaigns.Domain.Bloggers.LastNames;
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
            .HasConversion(firstName => firstName.Value, value => FirstName.Create(value).Value)
            .IsRequired();
            
        builder
            .Property(blogger => blogger.LastName)
            .HasConversion(lastName => lastName.Value, value => LastName.Create(value).Value)
            .IsRequired();
            
        builder
            .Property(blogger => blogger.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .IsRequired();
            
        builder
            .Property(blogger => blogger.BirthDate)
            .HasConversion(birthDate => birthDate.Value, value => BirthDate.Create(value).Value)
            .IsRequired();
        
        builder
            .Property(user => user.Bio)
            .HasMaxLength(Bio.MaxLength)
            .HasConversion(bio => bio.Value, value => Bio.Create(value).Value)
            .IsRequired(false);
            
        builder
            .HasOne(blogger => blogger.Pact)
            .WithOne(pact => pact.Blogger)
            .HasForeignKey<Pact>(pact => pact.BloggerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
