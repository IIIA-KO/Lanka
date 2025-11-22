using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Instagram.GetLinkingStatus;

internal sealed class
    GetInstagramLinkingStatusQueryHandler : IQueryHandler<GetInstagramLinkingStatusQuery, InstagramOperationStatus?>
{
    private readonly ICacheService _cacheService;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public GetInstagramLinkingStatusQueryHandler(
        ICacheService cacheService,
        IUserContext userContext,
        IUserRepository userRepository)
    {
        this._cacheService = cacheService;
        this._userContext = userContext;
        this._userRepository = userRepository;
    }

    public async Task<Result<InstagramOperationStatus?>> Handle(GetInstagramLinkingStatusQuery request,
        CancellationToken cancellationToken)
    {
        Guid userId = this._userContext.GetUserId();
        string cacheKey = $"instagram_linking_status_{userId}";

        User? user = await this._userRepository.GetByIdAsync(new UserId(userId), cancellationToken);

        if (user?.InstagramAccountLinkedOnUtc.HasValue ?? false)
        {
            InstagramOperationStatus status = await this._cacheService.GetOrCreateAsync(
                cacheKey,
                factory: _ => ValueTask.FromResult(
                    new InstagramOperationStatus(
                        InstagramOperationType.Linking,
                        InstagramOperationStatuses.Completed,
                        "Instagram account linked successfully.",
                        user.InstagramAccountLinkedOnUtc.Value.UtcDateTime,
                        user.InstagramAccountLinkedOnUtc.Value.UtcDateTime
                    )
                ),
                TimeSpan.FromMinutes(10),
                cancellationToken
            );

            return status;
        }

        InstagramOperationStatus? pendingStatus =
            await this._cacheService.GetAsync<InstagramOperationStatus>(cacheKey, cancellationToken);
        
        return pendingStatus;
    }
}
