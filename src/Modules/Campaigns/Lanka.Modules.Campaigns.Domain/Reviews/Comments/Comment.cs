using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Reviews.Comments;

public record Comment
{
    public const int MaxLength = 150;

    public string Value { get; init; }

    private Comment(string value)
    {
        this.Value = value;
    }

    public static Result<Comment> Create(string value)
    {
        Result validationResult = ValidateContentString(value);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Comment>(validationResult.Error);
        }

        return new Comment(value);
    }

    private static Result ValidateContentString(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return CommentErrors.Empty;
        }

        if (name.Length > MaxLength)
        {
            return CommentErrors.TooLong(MaxLength);
        }

        return Result.Success();
    }
}
