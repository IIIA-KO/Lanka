using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Instagram.Models;

namespace Lanka.Modules.Users.Application.Instagram.GetRenewalStatus;

internal sealed class GetInstagramRenewalStatusQueryHandler : IQueryHandler<GetInstagramRenewalStatusQuery, InstagramOperationStatus>
{
    private readonly IInstagramOperationStatusService _statusService;
    private readonly IUserContext _userContext;

    public GetInstagramRenewalStatusQueryHandler(
        IInstagramOperationStatusService statusService,
        IUserContext userContext)
    {
        this._statusService = statusService;
        this._userContext = userContext;
    }

    public async Task<Result<InstagramOperationStatus>> Handle(GetInstagramRenewalStatusQuery request, CancellationToken cancellationToken)
    {
        Guid userId = this._userContext.GetUserId();

        return await this._statusService.GetStatusAsync(userId, InstagramOperationType.Renewal, cancellationToken);
    }
}
