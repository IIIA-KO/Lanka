namespace Lanka.Modules.Analytics.Domain.IGAccounts;

public sealed class Category
{
    public static Category None => new("None");
    public static Category CookingAndFood => new("Cooking and Food");
    public static Category FashionAndStyle => new("Fashion and Style");
    public static Category ClothingAndFootwear => new("Clothing and Footwear");
    public static Category Horticulture => new("Horticulture");
    public static Category Animals => new("Animals");
    public static Category Cryptocurrency => new("Cryptocurrency");
    public static Category Technology => new("Technology");
    public static Category Travel => new("Travel");
    public static Category Education => new("Education");
    public static Category Fitness => new("Fitness");
    public static Category Art => new("Art");
    public static Category Photography => new("Photography");
    public static Category Music => new("Music");
    public static Category Sports => new("Sports");
    public static Category HealthAndWellness => new("Health and Wellness");
    public static Category Gaming => new("Gaming");
    public static Category Parenting => new("Parenting");
    public static Category DIYAndCrafts => new("DIY and Crafts");
    public static Category Literature => new("Literature");
    public static Category Science => new("Science");
    public static Category History => new("History");
    public static Category News => new("News");
    public static Category Politics => new("Politics");
    public static Category Finance => new("Finance");
    public static Category Environment => new("Environment");
    public static Category RealEstate => new("Real Estate");
    public static Category Automobiles => new("Automobiles");
    public static Category MoviesAndTV => new("Movies and TV");
    public static Category Comedy => new("Comedy");
    public static Category HomeDecor => new("Home Decor");
    public static Category Relationships => new("Relationships");
    public static Category SelfImprovement => new("Self Improvement");
    public static Category Entrepreneurship => new("Entrepreneurship");
    public static Category LegalAdvice => new("Legal Advice");
    public static Category Marketing => new("Marketing");
    public static Category MentalHealth => new("Mental Health");
    public static Category PersonalDevelopment => new("Personal Development");
    public static Category ReligionAndSpirituality => new("Religion and Spirituality");
    public static Category SocialMedia => new("Social Media");
    
    private Category(string name)
    {
        this.Name = name;
    }

    private Category() { }

    public string Name { get; private set; }
}
