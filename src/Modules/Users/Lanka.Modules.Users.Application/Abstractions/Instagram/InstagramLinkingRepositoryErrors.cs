using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Application.Abstractions.Instagram;

public static class InstagramLinkingRepositoryErrors
{
    public static Error AlreadyLinking =>
        Error.Conflict(
            "InstagramLinkingRepository.AlreadyLinking",
            "User with specified identified is already linking their Instagram account, please try again in a minute."
        );
}
