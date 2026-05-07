using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Chat;
using Lanka.Modules.Campaigns.Domain.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Chat;

internal sealed class ChatThreadConfiguration : IEntityTypeConfiguration<ChatThread>
{
    public void Configure(EntityTypeBuilder<ChatThread> builder)
    {
        builder.ToTable("chat_threads");

        builder.HasKey(thread => thread.Id);

        builder
            .Property(thread => thread.Id)
            .HasConversion(id => id.Value, value => new ChatThreadId(value))
            .IsRequired();

        builder
            .Property(thread => thread.ParticipantAId)
            .HasConversion(id => id.Value, value => new BloggerId(value))
            .HasColumnName("participant_a_id")
            .IsRequired();

        builder
            .Property(thread => thread.ParticipantBId)
            .HasConversion(id => id.Value, value => new BloggerId(value))
            .HasColumnName("participant_b_id")
            .IsRequired();

        builder
            .Property(thread => thread.CampaignId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? new CampaignId(value.Value) : null)
            .HasColumnName("campaign_id");

        builder
            .Property(thread => thread.OfferId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? new OfferId(value.Value) : null)
            .HasColumnName("offer_id");

        builder
            .Property(thread => thread.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(thread => thread.UpdatedAtUtc)
            .IsRequired();

        builder
            .HasIndex(thread => thread.ParticipantAId);

        builder
            .HasIndex(thread => thread.ParticipantBId);

        builder
            .HasIndex(thread => thread.CampaignId);

        builder
            .HasIndex(thread => thread.OfferId);
    }
}
