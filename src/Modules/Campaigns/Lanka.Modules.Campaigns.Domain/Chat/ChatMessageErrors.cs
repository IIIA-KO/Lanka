using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Chat;

public static class ChatMessageErrors
{
    public static readonly Error NotFound =
        Error.NotFound("ChatMessage.NotFound", "The chat message with the specified identifier was not found");

    public static readonly Error EmptyContent =
        Error.Validation("ChatMessage.EmptyContent", "Message content is required");

    public static readonly Error ContentTooLong =
        Error.Validation("ChatMessage.ContentTooLong", "Message content cannot exceed 4000 characters");

    public static readonly Error SystemMessageImmutable =
        Error.Failure("ChatMessage.SystemMessageImmutable", "System messages cannot be changed");

    public static readonly Error DeletedMessageImmutable =
        Error.Failure("ChatMessage.DeletedMessageImmutable", "Deleted messages cannot be changed");
}
