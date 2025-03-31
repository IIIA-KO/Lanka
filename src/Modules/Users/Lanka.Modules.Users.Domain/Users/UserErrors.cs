using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users;

public static class UserErrors
{
    public static Error NotFound =>
        Error.NotFound("Users.NotFound", $"The user with the specified identifier was not found");
}
