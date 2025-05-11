using Lanka.Modules.Campaigns.UnitTests.Abstractions;

namespace Lanka.Modules.Campaigns.UnitTests.Bloggers;

internal static class BloggerData
{
    public static string FirstName = "FirstName";

    public static string LastName = "LastName";

    public static string Email = "blogger@lanka.com";
    
    public static string Bio = "Bio";

    public static DateOnly BirthDate => BaseTest.Faker.Date.PastDateOnly(
        18,
        new DateOnly(DateTime.Now.Year - 18, 1, 1)
    );
}
