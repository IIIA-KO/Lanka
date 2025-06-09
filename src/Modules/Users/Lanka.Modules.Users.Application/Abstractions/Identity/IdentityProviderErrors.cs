using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
    public static Error EmailIsNotUnique =>
        Error.Conflict("Identity.EmailIsNotUnique", "The specified email is already taken.");

    public static Error AuthenticationFailed =>
        Error.Failure(
            "Keycloak.AuthenticationFailed",
            "Failed to acquire access token due to authentication failure"
        );

    public static Error FailedToTerminateSession =>
        Error.Failure(
            "User.FailedTerminateSession",
            "Failed to terminate session of the user with provided indentifier"
        );

    public static Error FailedToDeleteAccount =>
        Error.Problem("User.FailedDeleteAccount", "Failed to delete use account");

    public static Error InvalidCredentials =>
        Error.Unauthorized("User.InvalidCredentials", "The provided credentials were invalid");

    public static Error ExternalIdentityProviderAlreadyLinked =>
        Error.Conflict("User.ExternalIdentityProviderAlreadyLinked",
            "The external identity provider is already linked");
}
