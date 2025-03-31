using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
    public static readonly Error EmailIsNotUnique =
        Error.Conflict("Identity.EmailIsNotUnique", "The specified email is already taken.");

    public static readonly Error AuthenticationFailed =
        Error.Failure(
            "Keycloak.AuthenticationFailed",
            "Failed to acquire access token due to authentication failure"
        );
    
    public static readonly Error FailedToTerminateSession =
        Error.Failure(
            "User.FailedTerminateSession",
            "Failed to terminate session of the user with provided indentifier"
        );

    public static readonly Error FailedToDeleteAccount =
        Error.Problem("User.FailedDeleteAccount", "Failed to delete use account");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("User.InvalidCredentials", "The provided credentials were invalid");
}
