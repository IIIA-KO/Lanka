using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Reviews.Comments;

public static class CommentErrors
{
    public static readonly Error Empty =
        Error.Validation("Comment.Empty", "Comment cannot be empty.");

    public static Error TooLong(int maxLength) =>
        Error.Validation("Comment.TooLong", $"Comment cannot exceed {maxLength}.");
}
