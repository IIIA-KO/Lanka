using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts;

public static class InstagramAccountErrors
{
    public static Error NotFound => Error.NotFound(
        "InstagramAccount.NotFound",
        "The Instagram account with the specified identifier was not found."
    );

    public static Error TokenNotFound => Error.NotFound(
        "InstagramAccount.TokenNotFound",
        "The Instagram account token was not found. Please reconnect your Instagram account."
    );

    public static Error FailedToGetFacebookPage => Error.Failure(
        "InstagramAccount.FailedToGetFacebookPage",
        "Failed to retrieve the Facebook Business Page associated with the Instagram account."
    );

    public static Error IncorrectFacebookPagesCount => Error.Failure(
        "InstagramAccount.IncorrectFacebookPagesCount",
        "The number of selected Facebook Pages associated with the Instagram account is incorrect."
    );

    public static Error FailedToGetAdAccount => Error.Failure(
        "InstagramAccount.FailedToGetAdAccount",
        "Failed to retrieve the Advertisement Account associated with the Instagram account."
    );

    public static Error FailedToGetExpirationForAccessToken => Error.Unauthorized(
        "InstagramAccount.FailedToGetExpirationForAccessToken",
        "Failed to get expires_at property while retrieving access token"
    );

    public static Error Unexpected => Error.Failure(
        "InstagramAccount.Unexpected",
        "An unexpected error occurred while processing the Instagram account."
    );

    public static Error WrongInstagramAccount => Error.Failure(
        "InstagramAccount.WrongInstagramAccount",
        "The provided Instagram account is not associated with the current user."
    );
}
