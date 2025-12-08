using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.Categories;

public sealed class Category
{
    public const int MaxLength = 100;
    
    public static readonly Category None = new("None");
    public static readonly Category CookingAndFood = new("Cooking and Food");
    public static readonly Category FashionAndStyle = new("Fashion and Style");
    public static readonly Category ClothingAndFootwear = new("Clothing and Footwear");
    public static readonly Category Horticulture = new("Horticulture");
    public static readonly Category Animals = new("Animals");
    public static readonly Category Cryptocurrency = new("Cryptocurrency");
    public static readonly Category Technology = new("Technology");
    public static readonly Category Travel = new("Travel");
    public static readonly Category Education = new("Education");
    public static readonly Category Fitness = new("Fitness");
    public static readonly Category Art = new("Art");
    public static readonly Category Photography = new("Photography");
    public static readonly Category Music = new("Music");
    public static readonly Category Sports = new("Sports");
    public static readonly Category HealthAndWellness = new("Health and Wellness");
    public static readonly Category Gaming = new("Gaming");
    public static readonly Category Parenting = new("Parenting");
    public static readonly Category DIYAndCrafts = new("DIY and Crafts");
    public static readonly Category Literature = new("Literature");
    public static readonly Category Science = new("Science");
    public static readonly Category History = new("History");
    public static readonly Category News = new("News");
    public static readonly Category Politics = new("Politics");
    public static readonly Category Finance = new("Finance");
    public static readonly Category Environment = new("Environment");
    public static readonly Category RealEstate = new("Real Estate");
    public static readonly Category Automobiles = new("Automobiles");
    public static readonly Category MoviesAndTV = new("Movies and TV");
    public static readonly Category Comedy = new("Comedy");
    public static readonly Category HomeDecor = new("Home Decor");
    public static readonly Category Relationships = new("Relationships");
    public static readonly Category SelfImprovement = new("Self Improvement");
    public static readonly Category Entrepreneurship = new("Entrepreneurship");
    public static readonly Category LegalAdvice = new("Legal Advice");
    public static readonly Category Marketing = new("Marketing");
    public static readonly Category MentalHealth = new("Mental Health");
    public static readonly Category PersonalDevelopment = new("Personal Development");
    public static readonly Category ReligionAndSpirituality = new("Religion and Spirituality");
    public static readonly Category SocialMedia = new("Social Media");

    private static readonly Dictionary<string, Category> _categoryLookup =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { None.Name, None },
            { CookingAndFood.Name, CookingAndFood },
            { FashionAndStyle.Name, FashionAndStyle },
            { ClothingAndFootwear.Name, ClothingAndFootwear },
            { Horticulture.Name, Horticulture },
            { Animals.Name, Animals },
            { Cryptocurrency.Name, Cryptocurrency },
            { Technology.Name, Technology },
            { Travel.Name, Travel },
            { Education.Name, Education },
            { Fitness.Name, Fitness },
            { Art.Name, Art },
            { Photography.Name, Photography },
            { Music.Name, Music },
            { Sports.Name, Sports },
            { HealthAndWellness.Name, HealthAndWellness },
            { Gaming.Name, Gaming },
            { Parenting.Name, Parenting },
            { DIYAndCrafts.Name, DIYAndCrafts },
            { Literature.Name, Literature },
            { Science.Name, Science },
            { History.Name, History },
            { News.Name, News },
            { Politics.Name, Politics },
            { Finance.Name, Finance },
            { Environment.Name, Environment },
            { RealEstate.Name, RealEstate },
            { Automobiles.Name, Automobiles },
            { MoviesAndTV.Name, MoviesAndTV },
            { Comedy.Name, Comedy },
            { HomeDecor.Name, HomeDecor },
            { Relationships.Name, Relationships },
            { SelfImprovement.Name, SelfImprovement },
            { Entrepreneurship.Name, Entrepreneurship },
            { LegalAdvice.Name, LegalAdvice },
            { Marketing.Name, Marketing },
            { MentalHealth.Name, MentalHealth },
            { PersonalDevelopment.Name, PersonalDevelopment },
            { ReligionAndSpirituality.Name, ReligionAndSpirituality },
            { SocialMedia.Name, SocialMedia }
        };

    private Category(string name)
    {
        this.Name = name;
    }

    private Category() { }

    public string Name { get; private set; }

    public static Result<Category> FromName(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return Result.Failure<Category>(BloggerErrors.InvalidCategory);
        }

        string trimmedCategory = categoryName.Trim();

        if (_categoryLookup.TryGetValue(trimmedCategory, out Category category))
        {
            return category;
        }

        return Result.Failure<Category>(BloggerErrors.InvalidCategory);
    }
}
