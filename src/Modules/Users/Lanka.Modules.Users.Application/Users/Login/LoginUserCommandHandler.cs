using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Domain.Users.Emails;

namespace Lanka.Modules.Users.Application.Users.Login;

internal sealed class LoginUserCommandHandler
    : ICommandHandler<LoginUserCommand, AccessTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityProviderService _identityProviderService;

    public LoginUserCommandHandler(IUserRepository userRepository, IIdentityProviderService identityProviderService)
    {
        this._userRepository = userRepository;
        this._identityProviderService = identityProviderService;
    }

    public async Task<Result<AccessTokenResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await this._userRepository.GetByEmailAsync(Email.Create(request.Email).Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AccessTokenResponse>(UserErrors.NotFound);
        }

        Result<AccessTokenResponse> result = await this._identityProviderService.GetAccessTokenAsync(
            Email.Create(request.Email).Value,
            request.Password,
            cancellationToken
        );

        return result.IsFailure ? result : result.Value;
    }
}
