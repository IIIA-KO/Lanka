using Lanka.Modules.Campaigns.UnitTests.Abstractions;

namespace Lanka.Modules.Campaigns.UnitTests.Bloggers;

internal static class BloggerData
{
    public static string FirstName = BaseTest.Faker.Person.FirstName;

    public static string LastName = BaseTest.Faker.Person.LastName;

    public static string Email = BaseTest.Faker.Internet.Email();
    
    public static string Bio = BaseTest.Faker.Lorem.Sentences(3);

    public static DateOnly BirthDate => BaseTest.Faker.Date.PastDateOnly(
        18,
        new DateOnly(DateTime.Now.Year - 18, 1, 1)
    );
}
