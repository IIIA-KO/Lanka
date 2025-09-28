using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Instagram.Models;

namespace Lanka.Modules.Users.Application.Instagram.GetLinkingStatus;

internal sealed class GetInstagramLinkingStatusQueryHandler : IQueryHandler<GetInstagramLinkingStatusQuery, InstagramOperationStatus?>
{
    private readonly ICacheService _cacheService;
    private readonly IUserContext _userContext;

    public GetInstagramLinkingStatusQueryHandler(ICacheService cacheService, IUserContext userContext)
    {
        this._cacheService = cacheService;
        this._userContext = userContext;
    }

    public async Task<Result<InstagramOperationStatus?>> Handle(GetInstagramLinkingStatusQuery request, CancellationToken cancellationToken)
    {
        Guid userId = this._userContext.GetUserId();
        string cacheKey = $"instagram_linking_status_{userId}";

        InstagramOperationStatus? status = await this._cacheService.GetAsync<InstagramOperationStatus>(cacheKey, cancellationToken);

        return status;
    }
}
