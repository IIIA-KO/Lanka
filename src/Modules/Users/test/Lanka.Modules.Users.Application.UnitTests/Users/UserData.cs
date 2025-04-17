using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.UnitTests.Users;

internal static class UserData
{
    public static User CreateUser()
    {
        return User.Create(
            FirstName,
            LastName,
            Email,
            BirthDate,
            IdentityId
        ).Value;
    }

    public static string IdentityId => Guid.NewGuid().ToString();
    
    public static string Password => "P@ssw0rd";

    public static string Email => "email@lanka.com";

    public static string FirstName => "FirstName";

    public static string LastName => "LastName";

    public static DateOnly BirthDate => new(DateTime.Now.Year - 18, 1, 1);
    
    public static AccessTokenResponse Token => new (
        "access_token", 
        600, 
        "refresh_token", 
        1800
    );
}
