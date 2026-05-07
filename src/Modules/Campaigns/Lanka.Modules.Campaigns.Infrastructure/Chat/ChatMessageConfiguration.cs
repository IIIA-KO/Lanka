using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Chat;

internal sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_messages");

        builder.HasKey(message => message.Id);

        builder
            .Property(message => message.Id)
            .HasConversion(id => id.Value, value => new ChatMessageId(value))
            .IsRequired();

        builder
            .Property(message => message.ThreadId)
            .HasConversion(id => id.Value, value => new ChatThreadId(value))
            .IsRequired();

        builder
            .Property(message => message.SenderBloggerId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? new BloggerId(value.Value) : null)
            .HasColumnName("sender_blogger_id");

        builder
            .Property(message => message.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder
            .Property(message => message.IsSystem)
            .IsRequired();

        builder
            .Property(message => message.IsDeleted)
            .IsRequired();

        builder
            .Property(message => message.CreatedAtUtc)
            .IsRequired();

        builder
            .HasOne<ChatThread>()
            .WithMany()
            .HasForeignKey(message => message.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(message => new { message.ThreadId, message.CreatedAtUtc })
            .IsDescending(false, true);
    }
}
