using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.AdvertisementAccountIds;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.FacebookPageIds;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts;

public class InstagramAccount : Entity<InstagramAccountId>
{
    public const int MinFollowers = 100;

    public UserId UserId { get; private set; }

    public FacebookPageId FacebookPageId { get; private set; }

    public AdvertisementAccountId AdvertisementAccountId { get; private set; }

    public Category Category { get; private set; }

    public Metadata Metadata { get; private set; }

    public DateTimeOffset LastUpdatedAtUtc { get; }

    public Token? Token { get; set; }

    private InstagramAccount() { }

    private InstagramAccount(
        InstagramAccountId id,
        UserId userId,
        FacebookPageId facebookPageId,
        AdvertisementAccountId advertisementAccountId,
        Category category,
        Metadata metadata
    )
    {
        this.Id = id;
        this.UserId = userId;
        this.FacebookPageId = facebookPageId;
        this.AdvertisementAccountId = advertisementAccountId;
        this.Category = category;
        this.Metadata = metadata;
    }

    public static Result<InstagramAccount> Create(
        Guid userId,
        string fbPageId,
        string advertisementAccountId,
        Metadata metadata
    )
    {
        Result<(FacebookPageId, AdvertisementAccountId)> validationResult = Validate(fbPageId, advertisementAccountId);

        if (validationResult.IsFailure)
        {
            return Result.Failure<InstagramAccount>(validationResult.Error);
        }

        (FacebookPageId _fbPageId, AdvertisementAccountId _advertisementAccountId) = validationResult.Value;

        var igAccount = new InstagramAccount(
            InstagramAccountId.New(),
            new UserId(userId),
            _fbPageId,
            _advertisementAccountId,
            Category.None,
            metadata
        );

        igAccount.RaiseDomainEvent(
            new InstagramAccountDataFetchedDomainEvent(
                igAccount.UserId,
                igAccount.Metadata.UserName,
                igAccount.Metadata.Id
            )
        );

        return igAccount;
    }

    public void Update(Metadata metadata)
    {
        this.Metadata = metadata;
    }
    
    private static Result<(FacebookPageId, AdvertisementAccountId)> Validate(
        string fbPageId,
        string advertisementAccountId
    )
    {
        Result<FacebookPageId> fbPageIdResult = FacebookPageId.Create(fbPageId);
        Result<AdvertisementAccountId> adAccountIdResult = AdvertisementAccountId.Create(advertisementAccountId);

        if (fbPageIdResult.IsFailure || adAccountIdResult.IsFailure)
        {
            return Result.Failure<(FacebookPageId, AdvertisementAccountId)>(
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
