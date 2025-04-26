using Lanka.Modules.Users.Presentation.Users;

namespace Lanka.IntegrationTests.Users;

internal static class UserData
{
    public static DateOnly ValidBirthDate => DateOnly.FromDateTime(
        DateTime.Now.AddYears(-18).AddDays(-1)
    );

    public static string Password => "P@$$w0rd";

    public static RegisterUser.RegisterUserRequest RegisterTestUserRequest =>
        new()
        {
            Email = "test@lanka.com",
            Password = Password,
            FirstName = "first",
            LastName = "last",
            BirthDate = ValidBirthDate
        };
}
