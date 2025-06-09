using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Instagram.FinishInstagramLinking;

internal sealed class FinishInstagramLinkingCommandHandler
    : ICommandHandler<FinishInstagramLinkingCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityProviderService _identityProviderService;

    public FinishInstagramLinkingCommandHandler(
        IUserRepository userRepository,
        IIdentityProviderService identityProviderService
    )
    {
        this._userRepository = userRepository;
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
        
        return Result.Success();
    }
}
