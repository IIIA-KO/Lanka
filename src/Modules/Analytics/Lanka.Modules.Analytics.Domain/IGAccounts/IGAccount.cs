using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.IGAccounts.AdvertisementAccountIds;
using Lanka.Modules.Analytics.Domain.IGAccounts.FBPageIds;

namespace Lanka.Modules.Analytics.Domain.IGAccounts;

public class IGAccount : Entity<IGAccountId>
{
    public const int MinFollowers = 100;

    public BloggerId BloggerId { get; private set; }

    public FBPageId FBPageId { get; private set; }

    public AdvertisementAccountId AdvertisementAccountId { get; private set; }

    public Metadata Metadata { get; private set; }

    public DateTimeOffset LastUpdatedAtUtc { get; private set; }

    private IGAccount() { }

    private IGAccount(
        IGAccountId id,
        BloggerId bloggerId,
        FBPageId fbPageId,
        AdvertisementAccountId advertisementAccountId,
        Metadata metadata
    )
    {
        this.Id = id;
        this.BloggerId = bloggerId;
        this.FBPageId = fbPageId;
        this.AdvertisementAccountId = advertisementAccountId;
        this.Metadata = metadata;
    }

    public static Result<IGAccount> Create(
        Guid bloggerId,
        string fbPageId,
        string advertisementAccountId,
        Metadata metadata
    )
    {
        Result<(FBPageId, AdvertisementAccountId)> validationResult = Validate(fbPageId, advertisementAccountId);

        if (validationResult.IsFailure)
        {
            return Result.Failure<IGAccount>(validationResult.Error);
        }


        (FBPageId _fbPageId, AdvertisementAccountId _advertisementAccountId) = validationResult.Value;

        var igAccount = new IGAccount(
            IGAccountId.New(),
            new BloggerId(bloggerId),
            _fbPageId,
            _advertisementAccountId,
            metadata
        );

        return igAccount;
    }

    private static Result<(FBPageId, AdvertisementAccountId)> Validate(
        string fbPageId,
        string advertisementAccountId
    )
    {
        Result<FBPageId> fbPageIdResult = FBPageId.Create(fbPageId);
        Result<AdvertisementAccountId> adAccountIdResult = AdvertisementAccountId.Create(advertisementAccountId);

        if (fbPageIdResult.IsFailure
            || adAccountIdResult.IsFailure)
        {
            return Result.Failure<(FBPageId, AdvertisementAccountId)>(
                ValidationError.FromResults([
                        fbPageIdResult.Error,
                        adAccountIdResult.Error
                    ]
                )
            );
        }

        return (fbPageIdResult.Value, adAccountIdResult.Value);
    }
}
