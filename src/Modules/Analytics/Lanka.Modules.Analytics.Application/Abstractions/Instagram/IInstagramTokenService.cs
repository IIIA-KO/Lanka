using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramTokenService
{
    Task<Result<FacebookTokenResponse>> GetAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    Task<Result<FacebookTokenResponse>> RenewAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    );
}
