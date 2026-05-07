using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Notifications;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder
            .Property(n => n.Id)
            .HasConversion(id => id.Value, value => new NotificationId(value))
            .IsRequired();

        builder
            .Property(n => n.RecipientId)
            .HasConversion(id => id.Value, value => new BloggerId(value))
            .HasColumnName("recipient_blogger_id")
            .IsRequired();

        builder
            .Property(n => n.CampaignId)
            .IsRequired();

        builder
            .Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(n => n.Body)
            .HasMaxLength(500)
            .IsRequired();

        builder
            .Property(n => n.IsRead)
            .IsRequired();

        builder
            .Property(n => n.CreatedAtUtc)
            .IsRequired();
    }
}
