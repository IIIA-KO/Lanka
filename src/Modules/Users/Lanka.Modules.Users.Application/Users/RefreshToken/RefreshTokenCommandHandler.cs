using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Login;

namespace Lanka.Modules.Users.Application.Users.RefreshToken;

internal sealed class RefreshTokenCommandHandler 
    : ICommandHandler<RefreshTokenCommand, AccessTokenResponse>
{
    private readonly IIdentityProviderService _identityProviderService;

    public RefreshTokenCommandHandler(IIdentityProviderService identityProviderService)
    {
        this._identityProviderService = identityProviderService;
    }
    
    public async Task<Result<AccessTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Result<AccessTokenResponse> result = await this._identityProviderService
            .RefreshTokenAsync(request.RefreshToken, cancellationToken);

        return result.IsFailure 
            ? Result.Failure<AccessTokenResponse>(IdentityProviderErrors.InvalidCredentials) 
            : result;
    }
}
