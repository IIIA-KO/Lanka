using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Instagram.GetLinkingStatus;

internal sealed class
    GetInstagramLinkingStatusQueryHandler : IQueryHandler<GetInstagramLinkingStatusQuery, InstagramOperationStatus>
{
    private readonly IInstagramOperationStatusService _statusService;
    private readonly ICacheService _cacheService;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IUnitOfWork _unitOfWork;

    public GetInstagramLinkingStatusQueryHandler(
        IInstagramOperationStatusService statusService,
        ICacheService cacheService,
        IUserContext userContext,
        IUserRepository userRepository,
        IIdentityProviderService identityProviderService,
        IUnitOfWork unitOfWork)
    {
        this._statusService = statusService;
        this._cacheService = cacheService;
        this._userContext = userContext;
        this._userRepository = userRepository;
        this._identityProviderService = identityProviderService;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<InstagramOperationStatus>> Handle(GetInstagramLinkingStatusQuery request,
        CancellationToken cancellationToken)
    {
        Guid userId = this._userContext.GetUserId();

        User? user = await this._userRepository.GetByIdAsync(new UserId(userId), cancellationToken);

        if (user is null)
        {
            return Result.Failure<InstagramOperationStatus>(UserErrors.NotFound);
        }

        // If user already has Instagram linked, return completed status directly.
        // Use SetAsync to overwrite any stale cache entry (e.g. "pending" from a just-completed saga).
        if (user.InstagramAccountLinkedOnUtc.HasValue)
        {
            return await this.SetAndReturnCompletedStatusAsync(
                userId,
                user.InstagramAccountLinkedOnUtc.Value.UtcDateTime,
                cancellationToken);
        }

        // Fallback: check Keycloak for external identity link.
        // This handles cases where the linking completed but InstagramAccountLinkedOnUtc wasn't persisted,
        // or after access renewal when the cache entry has expired.
        bool isLinkedInKeycloak = await this._identityProviderService.IsExternalAccountLinkedAsync(
            user.IdentityId,
            ProviderName.Instagram,
            cancellationToken);

        if (isLinkedInKeycloak)
        {
            // Backfill the missing field so future checks are fast
            user.InstagramAccountLinkedOnUtc = DateTimeOffset.UtcNow;
            await this._unitOfWork.SaveChangesAsync(cancellationToken);

            return await this.SetAndReturnCompletedStatusAsync(userId, DateTime.UtcNow, cancellationToken);
        }

        // Otherwise, get the current operation status from cache
        return await this._statusService.GetStatusAsync(userId, InstagramOperationType.Linking, cancellationToken);
    }

    private async Task<InstagramOperationStatus> SetAndReturnCompletedStatusAsync(
        Guid userId,
        DateTime completedAt,
        CancellationToken cancellationToken)
    {
        var completedStatus = new InstagramOperationStatus(
            InstagramOperationType.Linking,
            InstagramOperationStatuses.Completed,
            "Instagram account linked successfully.",
            completedAt,
            completedAt
        );

        string cacheKey = IInstagramOperationStatusService.GetCacheKey(userId, InstagramOperationType.Linking);
        await this._cacheService.SetAsync(cacheKey, completedStatus, TimeSpan.FromMinutes(10), cancellationToken);

        return completedStatus;
    }
}
