using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

namespace Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;

internal sealed class FetchInstagramAccountDataCommandHandler
    : ICommandHandler<FetchInstagramAccountDataCommand>
{
    private readonly IFacebookService _facebookService;
    private readonly IInstagramAccountsService _instagramAccountsService;
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FetchInstagramAccountDataCommandHandler(
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

    public async Task<Result> Handle(FetchInstagramAccountDataCommand request, CancellationToken cancellationToken)
    {
        Result<FacebookTokenResponse> fbTokenResult = await this._facebookService.GetAccessTokenAsync(
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

        if (igUserInfo!.IsFailure)
        {
            return Result.Failure(igUserInfo.Error);
        }

        Result<InstagramAccount> igAccountResult = igUserInfo.Value.CreateInstagramAccount(new UserId(request.UserId));

        if (igAccountResult.IsFailure)
        {
            return Result.Failure(igAccountResult.Error);
        }

        InstagramAccount instagramAccount = igAccountResult.Value;

        this._instagramAccountRepository.Add(instagramAccount);

        Result<Token> tokenResult = Token.Create(
            request.UserId,
            fbTokenResult.Value.AccessToken,
            fbTokenResult.Value.ExpiresAtUtc,
            instagramAccount.Id
        );

        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error);
        }

        this._tokenRepository.Add(tokenResult.Value);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
