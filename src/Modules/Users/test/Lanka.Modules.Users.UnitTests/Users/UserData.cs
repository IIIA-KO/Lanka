using Lanka.Modules.Users.UnitTests.Abstractions;

namespace Lanka.Modules.Users.UnitTests.Users;

internal static class UserData
{
    public static string FirstName => BaseTest.Faker.Person.FirstName;

    public static string LastName => BaseTest.Faker.Person.LastName;

    public static string Email => BaseTest.Faker.Person.Email;

    public static DateOnly BirthDate => BaseTest.Faker.Date.PastDateOnly(
        18,
        new DateOnly(DateTime.Now.Year - 18, 1, 1)
    );
}
