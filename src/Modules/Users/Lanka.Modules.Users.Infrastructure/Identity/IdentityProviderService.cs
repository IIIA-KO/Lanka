using System.Net;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Infrastructure.Identity
{
    internal sealed class IdentityProviderService : IIdentityProviderService
    {
        private const string PasswordCredentialType = "Password";
        
        private readonly KeyCloakClient _keyCloakClient;
        private readonly ILogger<IdentityProviderService> _logger;

        public IdentityProviderService(KeyCloakClient keyCloakClient, ILogger<IdentityProviderService> logger)
        {
            this._keyCloakClient = keyCloakClient;
            this._logger = logger;
        }

        public async Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
        {
            var userRepresentation = new UserRepresentation(
                user.Email,
                user.Email,
                user.FirstName,
                user.LastName,
                true,
                true,
                [new CredentialRepresentation(PasswordCredentialType, user.Password, false)]);

            try
            {
                string identityId = await this._keyCloakClient.RegisterUserAsync(userRepresentation, cancellationToken);

                return identityId;
            }
            catch (HttpRequestException exception)
                when(exception.StatusCode == HttpStatusCode.Conflict)
            {
                this._logger.LogError(exception, "Failed to register user");

                return Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique);
            }
        }
    }
}
