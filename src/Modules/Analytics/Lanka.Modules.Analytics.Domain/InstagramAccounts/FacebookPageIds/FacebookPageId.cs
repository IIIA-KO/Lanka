using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.FacebookPageIds;

public sealed record FacebookPageId
{
    public string Value { get; init; }

    private FacebookPageId(string value)
    {
        this.Value = value;
    }

    public static Result<FacebookPageId> Create(string fbPageId)
    {
        Result validationResult = ValidateFbPageIdString(fbPageId);

        if (validationResult.IsFailure)
        {
            return Result.Failure<FacebookPageId>(validationResult.Error);
        }

        return new FacebookPageId(fbPageId);
    }

    private static Result ValidateFbPageIdString(string fbPageId)
    {
        if (string.IsNullOrWhiteSpace(fbPageId))
        {
            return FacebookPageIdErrors.Empty;
        }

        return Result.Success();
    }
}
