using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.GetPosts;

internal sealed class GetPostsQueryHandler : IQueryHandler<GetPostsQuery, PostsResponse>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IInstagramPostService _instagramPostService;

    public GetPostsQueryHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IInstagramPostService instagramPostService
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._instagramPostService = instagramPostService;
    }

    public async Task<Result<PostsResponse>> Handle(
        GetPostsQuery request,
        CancellationToken cancellationToken
    )
    {
        InstagramAccount? account = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (account is null)
        {
            return Result.Failure<PostsResponse>(InstagramAccountErrors.NotFound);
        }

        if (account.Token is null)
        {
            return Result.Failure<PostsResponse>(InstagramAccountErrors.TokenNotFound);
        }

        Result<PostsResponse> result = await this._instagramPostService.GetUserPostsWithInsights(
            account,
            request.Limit,
            request.CursorType ?? string.Empty,
            request.Cursor ?? string.Empty,
            cancellationToken
        );
        
        if (result.IsFailure)
        {
            return Result.Failure<PostsResponse>(result.Error);
        }

        return result.Value;
    }
}
