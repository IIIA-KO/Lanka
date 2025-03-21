using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
    public static readonly Error EmailIsNotUnique =
        Error.Conflict("Identity.EmailIsNotUnique", "The specified email is already taken.");
}