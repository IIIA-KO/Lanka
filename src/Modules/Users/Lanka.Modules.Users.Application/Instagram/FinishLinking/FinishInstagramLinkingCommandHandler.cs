using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Instagram.FinishLinking;

internal sealed class FinishInstagramLinkingCommandHandler
    : ICommandHandler<FinishInstagramLinkingCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityProviderService _identityProviderService;

    public FinishInstagramLinkingCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IIdentityProviderService identityProviderService
    )
    {
        this._userRepository = userRepository;
        this._unitOfWork = unitOfWork;
        this._identityProviderService = identityProviderService;
    }

    public async Task<Result> Handle(FinishInstagramLinkingCommand request, CancellationToken cancellationToken)
    {
        User? user = await this._userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        Result result = await this._identityProviderService.LinkExternalAccountToUserAsync(
            user.IdentityId,
            ProviderName.Instagram,
            request.IgId,
            request.Username,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return result;
        }

        user.InstagramAccountLinkedOnUtc = DateTimeOffset.UtcNow;
        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
