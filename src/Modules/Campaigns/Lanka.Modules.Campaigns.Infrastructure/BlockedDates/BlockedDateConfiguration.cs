using Lanka.Modules.Campaigns.Domain.BlockedDates;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.BlockedDates
{
    public sealed class BlockedDateConfiguration : IEntityTypeConfiguration<BlockedDate>
    {
        public void Configure(EntityTypeBuilder<BlockedDate> builder)
        {
            builder.ToTable("blocked_dates");
            
            builder.HasKey(blockedDate => blockedDate.Id);

            builder
                .HasIndex(blockedDate => new { blockedDate.BloggerId, blockedDate.Date })
                .IsUnique();
            
            builder
                .Property(blockedDate => blockedDate.Id)
                .HasConversion(id => id.Value, id => new BlockedDateId(id));

            builder
                .Property(blockedDate => blockedDate.BloggerId)
                .HasConversion(bloggerId => bloggerId.Value, value => new BloggerId(value))
                .IsRequired();

            builder
                .HasOne(blockedDate => blockedDate.Blogger)
                .WithMany()
                .HasForeignKey(blockedDate => blockedDate.BloggerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder
                .Property(blockedDate => blockedDate.Date)
                .IsRequired();
        }
    }
}
