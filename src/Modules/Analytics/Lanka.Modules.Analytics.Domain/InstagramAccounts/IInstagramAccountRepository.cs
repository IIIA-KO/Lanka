namespace Lanka.Modules.Analytics.Domain.InstagramAccounts;

public interface IInstagramAccountRepository
{
    Task<InstagramAccount?> GetByIdAsync(
        InstagramAccountId instagramAccountId,
        CancellationToken cancellationToken = default
    );

    Task<InstagramAccount?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default
    );

    Task<InstagramAccount?> GetByUserIdWithTokenAsync(
        UserId userId,
        CancellationToken cancellationToken = default
    );

    void Add(InstagramAccount instagramAccount);

    void Remove(InstagramAccount instagramAccount);
}
