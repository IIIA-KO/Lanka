using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public static class ChatThreadErrors
{
    public static readonly Error NotFound =
        Error.NotFound("ChatThread.NotFound", "The chat thread with the specified identifier was not found");

    public static readonly Error SameParticipant =
        Error.Conflict("ChatThread.SameParticipant", "A chat requires two different participants");
}
