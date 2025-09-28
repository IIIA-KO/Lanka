using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Instagram.Models;

namespace Lanka.Modules.Users.Application.Instagram.GetRenewalStatus;

internal sealed class GetInstagramRenewalStatusQueryHandler : IQueryHandler<GetInstagramRenewalStatusQuery, InstagramOperationStatus?>
{
    private readonly ICacheService _cacheService;
    private readonly IUserContext _userContext;

    public GetInstagramRenewalStatusQueryHandler(ICacheService cacheService, IUserContext userContext)
    {
        this._cacheService = cacheService;
        this._userContext = userContext;
    }

    public async Task<Result<InstagramOperationStatus?>> Handle(GetInstagramRenewalStatusQuery request, CancellationToken cancellationToken)
    {
        Guid userId = this._userContext.GetUserId();
        string cacheKey = $"instagram_renewal_status_{userId}";

        InstagramOperationStatus? status = await this._cacheService.GetAsync<InstagramOperationStatus>(cacheKey, cancellationToken);

        return status;
    }
}
