using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Models;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IFacebookService
{
    Task<Result<string>> GetFacebookPageIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );

    Task<Result<string>> GetAdAccountIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );

    Task<Result<FacebookTokenResponse>> GetAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    Task<Result<FacebookTokenResponse>> RenewAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    );
    
    Task<FacebookUserInfo?> GetFacebookUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );
}
