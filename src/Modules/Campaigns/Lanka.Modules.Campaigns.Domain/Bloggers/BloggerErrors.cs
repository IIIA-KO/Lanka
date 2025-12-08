using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public static class BloggerErrors
{
    public static readonly Error NotFound =
        Error.NotFound(
            "Blogger.NotFound",
            "The blogger with the specified identifier was not found"
        );
    
    public static Error InvalidCategory => Error.Validation(
        "Blogger.InvalidCategory",
        "The provided account category is not supported."
    );
}
