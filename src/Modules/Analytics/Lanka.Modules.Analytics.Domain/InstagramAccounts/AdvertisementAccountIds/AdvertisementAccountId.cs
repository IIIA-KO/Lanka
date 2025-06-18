using Lanka.Common.Domain;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.AdvertisementAccountIds;

public sealed record AdvertisementAccountId
{
    public string Value { get; init; }

    private AdvertisementAccountId(string value)
    {
        this.Value = value;
    }

    public static Result<AdvertisementAccountId> Create(string advertisementAccountId)
    {
        Result validationResult = ValidateAdvertisementAccountIdString(advertisementAccountId);

        if (validationResult.IsFailure)
        {
            return Result.Failure<AdvertisementAccountId>(validationResult.Error);
        }

        return new AdvertisementAccountId(advertisementAccountId);
    }

    private static Result ValidateAdvertisementAccountIdString(string advertisementAccountId)
    {
        if (string.IsNullOrWhiteSpace(advertisementAccountId))
        {
            return AdvertisementAccountIdErrors.Empty;
        }

        return Result.Success();
    }
}
