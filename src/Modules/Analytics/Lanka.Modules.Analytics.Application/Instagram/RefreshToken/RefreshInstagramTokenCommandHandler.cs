using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Metadatas;
using Lanka.Modules.Analytics.Domain.Tokens;

namespace Lanka.Modules.Analytics.Application.Instagram.RefreshToken;

internal sealed class RefreshInstagramTokenCommandHandler
    : ICommandHandler<RefreshInstagramTokenCommand>
{
    private readonly IFacebookService _facebookService;
    private readonly IInstagramAccountsService _instagramAccountsService;
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshInstagramTokenCommandHandler(
        IFacebookService facebookService,
        IInstagramAccountsService instagramAccountsService,
        IInstagramAccountRepository instagramAccountRepository,
        ITokenRepository tokenRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._facebookService = facebookService;
        this._instagramAccountsService = instagramAccountsService;
        this._instagramAccountRepository = instagramAccountRepository;
        this._tokenRepository = tokenRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RefreshInstagramTokenCommand request, CancellationToken cancellationToken)
    {
        Result<FacebookTokenResponse> fbTokenResult = await this._facebookService.RenewAccessTokenAsync(
            request.Code,
            cancellationToken
        );

        if (fbTokenResult.IsFailure)
        {
            return Result.Failure(fbTokenResult.Error);
        }

        Result<InstagramUserInfo> igUserInfo = await this._instagramAccountsService.GetUserInfoAsync(
            fbTokenResult.Value.AccessToken,
            cancellationToken
        );

        if (igUserInfo.IsFailure)
        {
            return Result.Failure(igUserInfo.Error);
        }

        InstagramUserInfo fetchedInstagramAccount = igUserInfo.Value;

        InstagramAccount? existingInstagramAccount = await this._instagramAccountRepository.GetByUserIdAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (existingInstagramAccount is null)
        {
            return Result.Failure(InstagramAccountErrors.NotFound);
        }

        if (existingInstagramAccount.Metadata.Id != fetchedInstagramAccount.BusinessDiscovery.Id)
        {
            return Result.Failure(InstagramAccountErrors.WrongInstagramAccount);
        }

        Result<Metadata> metadataResult = Metadata.Create(
            fetchedInstagramAccount.BusinessDiscovery.Id,
            fetchedInstagramAccount.BusinessDiscovery.IgId,
            fetchedInstagramAccount.BusinessDiscovery.Username,
            fetchedInstagramAccount.BusinessDiscovery.FollowersCount,
            fetchedInstagramAccount.BusinessDiscovery.MediaCount
        );

        if (metadataResult.IsFailure)
        {
            return metadataResult;
        }

        existingInstagramAccount.Update(metadataResult.Value);

        Token existingToken = await this._tokenRepository.GetByUserIdAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (existingToken is null)
        {
            return Result.Failure(TokenErrors.NotFound);
        }

        existingToken.Update(fbTokenResult.Value.AccessToken, fbTokenResult.Value.ExpiresAtUtc);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
