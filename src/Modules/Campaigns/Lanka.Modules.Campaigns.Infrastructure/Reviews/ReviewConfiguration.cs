using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Reviews;
using Lanka.Modules.Campaigns.Domain.Reviews.Comments;
using Lanka.Modules.Campaigns.Domain.Reviews.Ratings;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Campaigns.Infrastructure.Reviews;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasIndex(review => review.Id);

        builder
            .Property(review => review.Id)
            .HasConversion(id => id.Value, value => new ReviewId(value));

        builder
            .Property(review => review.Rating)
            .HasConversion(rating => rating.Value, value => Rating.Create(value).Value);

        builder
            .Property(review => review.Comment)
            .HasConversion(comment => comment.Value, value => Comment.Create(value).Value);

        builder
            .HasOne<Offer>()
            .WithMany()
            .HasForeignKey(review => review.OfferId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Blogger>()
            .WithMany()
            .HasForeignKey(review => review.ClientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Blogger>()
            .WithMany()
            .HasForeignKey(review => review.CreatorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(review => review.CampaignId)
            .IsRequired();
    }
}
