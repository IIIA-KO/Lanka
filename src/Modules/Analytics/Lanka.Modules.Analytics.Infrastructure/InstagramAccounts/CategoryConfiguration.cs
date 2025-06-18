using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Analytics.Infrastructure.InstagramAccounts;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Name);

        builder.Property(category => category.Name).HasMaxLength(100);

        builder
            .HasMany<InstagramAccount>()
            .WithOne(igAccount => igAccount.Category);

        builder.HasData(
            Category.None,
            Category.CookingAndFood,
            Category.FashionAndStyle,
            Category.ClothingAndFootwear,
            Category.Horticulture,
            Category.Animals,
            Category.Cryptocurrency,
            Category.Technology,
            Category.Travel,
            Category.Education,
            Category.Fitness,
            Category.Art,
            Category.Photography,
            Category.Music,
            Category.Sports,
            Category.HealthAndWellness,
            Category.Gaming,
            Category.Parenting,
            Category.DIYAndCrafts,
            Category.Literature,
            Category.Science,
            Category.History,
            Category.News,
            Category.Politics,
            Category.Finance,
            Category.Environment,
            Category.RealEstate,
            Category.Automobiles,
            Category.MoviesAndTV,
            Category.Comedy,
            Category.HomeDecor,
            Category.Relationships,
            Category.SelfImprovement,
            Category.Entrepreneurship,
            Category.LegalAdvice,
            Category.Marketing,
            Category.MentalHealth,
            Category.PersonalDevelopment,
            Category.ReligionAndSpirituality,
            Category.SocialMedia
        );
    }
}
