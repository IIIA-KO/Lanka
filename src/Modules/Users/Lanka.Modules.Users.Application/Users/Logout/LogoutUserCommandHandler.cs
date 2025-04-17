using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Users.Logout;

internal sealed class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityProviderService _identityProviderService;

    public LogoutUserCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IIdentityProviderService identityProviderService
    )
    {
        this._userContext = userContext;
        this._userRepository = userRepository;
        this._identityProviderService = identityProviderService;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await this._userRepository.GetByIdAsync(
            new UserId(this._userContext.GetUserId()),
            cancellationToken
        );
        
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        return await this._identityProviderService.TerminateUserSessionAsync(
            user.IdentityId,
            cancellationToken
        );
    }
}
