using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Abstractions.Instagram;
using Lanka.Modules.Users.Application.Instagram.Models;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Instagram.Link;

internal sealed class LinkInstagramAccountCommandHandler
    : ICommandHandler<LinkInstagramAccountCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IInstagramOperationStatusService _statusService;
    private readonly IUnitOfWork _unitOfWork;

    public LinkInstagramAccountCommandHandler(
        IUserRepository userRepository,
        IUserContext userContext,
        IIdentityProviderService identityProviderService,
        IInstagramOperationStatusService statusService,
        IUnitOfWork unitOfWork)
    {
        this._userRepository = userRepository;
        this._userContext = userContext;
        this._identityProviderService = identityProviderService;
        this._statusService = statusService;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        LinkInstagramAccountCommand request,
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

        if (isLinked)
        {
            return Result.Failure(IdentityProviderErrors.ExternalIdentityProviderAlreadyLinked);
        }

        InstagramOperationStatus existingStatus = await this._statusService.GetStatusAsync(
            user.Id.Value, InstagramOperationType.Linking, cancellationToken);

        bool isActivelyInProgress = existingStatus.Status is
            InstagramOperationStatuses.Pending or InstagramOperationStatuses.Processing;

        if (isActivelyInProgress)
        {
            return Result.Failure(InstagramLinkingRepositoryErrors.AlreadyLinking);
        }

        await this._statusService.SetStatusAsync(
            user.Id.Value,
            InstagramOperationType.Linking,
            InstagramOperationStatuses.Pending,
            "Instagram linking started",
            cancellationToken: cancellationToken
        );

        user.LinkInstagramAccount(request.Code);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
