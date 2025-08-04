using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.AdvertisementAccountIds;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.FacebookPageIds;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Metadatas;
using Lanka.Modules.Analytics.Domain.Tokens;

namespace Lanka.Modules.Analytics.Domain.InstagramAccounts;

public class InstagramAccount : Entity<InstagramAccountId>
{
    public UserId UserId { get; private set; }

    public FacebookPageId FacebookPageId { get; private set; }

    public AdvertisementAccountId AdvertisementAccountId { get; private set; }

    public Category Category { get; private set; }

    public Metadata Metadata { get; private set; }

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
        string businessDiscoveryId,
        long businessDiscoveryIgId,
        string businessDiscoveryUsername,
        int businessDiscoveryFollowersCount,
        int businessDiscoveryMediaCount
    )
    {
        Result<(FacebookPageId, AdvertisementAccountId, Metadata)> validationResult =
            Validate(
                fbPageId,
                advertisementAccountId,
                businessDiscoveryId,
                businessDiscoveryIgId,
                businessDiscoveryUsername,
                businessDiscoveryFollowersCount,
                businessDiscoveryMediaCount
            );

        if (validationResult.IsFailure)
        {
            return Result.Failure<InstagramAccount>(validationResult.Error);
        }

        (FacebookPageId _fbPageId, AdvertisementAccountId _advertisementAccountId, Metadata metadata) =
            validationResult.Value;

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
                igAccount.Metadata.FollowersCount,
                igAccount.Metadata.MediaCount,
                igAccount.Metadata.Id
            )
        );

        return igAccount;
    }

    public void Update(Metadata metadata)
    {
        this.Metadata = metadata;
    }

    private static Result<(FacebookPageId, AdvertisementAccountId, Metadata)> Validate(
        string fbPageId,
        string advertisementAccountId,
        string businessDiscoveryId,
        long businessDiscoveryIgId,
        string businessDiscoveryUsername,
        int businessDiscoveryFollowersCount,
        int businessDiscoveryMediaCount
    )
    {
        Result<FacebookPageId> fbPageIdResult = FacebookPageId.Create(fbPageId);
        Result<AdvertisementAccountId> adAccountIdResult = AdvertisementAccountId.Create(advertisementAccountId);
        Result<Metadata> metadataResult = Metadata.Create(
            businessDiscoveryId,
            businessDiscoveryIgId,
            businessDiscoveryUsername,
            businessDiscoveryFollowersCount,
            businessDiscoveryMediaCount
        );

        return new ValidationBuilder()
            .Add(fbPageIdResult)
            .Add(adAccountIdResult)
            .Add(metadataResult)
            .Build(() =>
                (
                    fbPageIdResult.Value,
                    adAccountIdResult.Value,
                    metadataResult.Value
                )
            );
    }
}
