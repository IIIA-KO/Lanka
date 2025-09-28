using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Instagram.RenewAccess;

internal sealed class RenewInstagramCommandHandler
    : ICommandHandler<RenewInstagramAccessCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public RenewInstagramCommandHandler(
        IUserRepository userRepository,
        IUserContext userContext,
        IIdentityProviderService identityProviderService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork
    )
    {
        this._userRepository = userRepository;
        this._userContext = userContext;
        this._identityProviderService = identityProviderService;
        this._cacheService = cacheService;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RenewInstagramAccessCommand request,
        CancellationToken cancellationToken
    )
    {
        User user = await this._userRepository.GetByIdAsync(
            new UserId(this._userContext.GetUserId()),
            cancellationToken
        );

        bool isLinked = await this._identityProviderService.IsExternalAccountLinkedAsync(
            user!.IdentityId,
            ProviderName.Instagram,
            cancellationToken
        );

        if (!isLinked)
        {
            return Result.Failure(IdentityProviderErrors.InstagramAccountNotLinked);
        }

        var status = new InstagramOperationStatus(
            InstagramOperationType.Renewal,
            InstagramOperationStatuses.Pending,
            "Instagram access renewal started",
            DateTime.UtcNow
        );

        string cacheKey = $"instagram_renewal_status_{user.Id.Value}";
        await this._cacheService.SetAsync(cacheKey, status, TimeSpan.FromMinutes(10), cancellationToken);

        user.RenewInstagramAccess(request.Code);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
