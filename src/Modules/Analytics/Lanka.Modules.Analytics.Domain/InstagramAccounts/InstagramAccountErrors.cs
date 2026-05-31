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
        "We couldn't read your Facebook Page. In the Facebook dialog, grant access to the Page that is connected to the Instagram account you want to link."
    );

    public static Error IncorrectFacebookPagesCount => Error.Failure(
        "InstagramAccount.IncorrectFacebookPagesCount",
        "Select exactly one Facebook Page during the Facebook dialog — the one linked to the Instagram account you want to connect. Granting more than one (or none) prevents us from identifying the right account."
    );

    public static Error FailedToGetAdAccount => Error.Failure(
        "InstagramAccount.FailedToGetAdAccount",
        "We couldn't find an ad account for the selected business. Make sure the business you picked owns an ad account and that you granted ads access during the Facebook dialog."
    );

    public static Error FailedToGetExpirationForAccessToken => Error.Unauthorized(
        "InstagramAccount.FailedToGetExpirationForAccessToken",
        "We couldn't validate your Facebook access token. Please try linking again."
    );

    public static Error Unexpected => Error.Failure(
        "InstagramAccount.Unexpected",
        "We couldn't read that Instagram account. Make sure the Facebook Page, business, and Instagram account you selected all belong together, and that the Instagram account is a Business or Creator account."
    );

    public static Error WrongInstagramAccount => Error.Failure(
        "InstagramAccount.WrongInstagramAccount",
        "The selected Instagram account isn't connected to the Facebook Page you chose. Pick the Page that is linked to your Instagram account."
    );
}
