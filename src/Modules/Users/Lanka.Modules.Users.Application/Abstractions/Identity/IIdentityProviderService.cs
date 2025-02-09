using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Application.Abstractions.Identity
{
    public interface IIdentityProviderService
    {
        Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
    }
}
