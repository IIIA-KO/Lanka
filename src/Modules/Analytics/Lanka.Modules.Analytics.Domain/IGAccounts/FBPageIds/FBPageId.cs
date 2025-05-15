using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.IGAccounts.FBPageIds;

public sealed record FBPageId
{
    public string Value { get; init; }

    private FBPageId(string value)
    {
        this.Value = value;
    }

    public static Result<FBPageId> Create(string fbPageId)
    {
        Result validationResult = ValidateFBPageIdString(fbPageId);

        if (validationResult.IsFailure)
        {
            return Result.Failure<FBPageId>(validationResult.Error);
        }

        return new FBPageId(fbPageId);
    }

    private static Result ValidateFBPageIdString(string fbPageId)
    {
        if (string.IsNullOrWhiteSpace(fbPageId))
        {
            return FBPageIdErrors.Empty;
        }

        return Result.Success();
    }
}
