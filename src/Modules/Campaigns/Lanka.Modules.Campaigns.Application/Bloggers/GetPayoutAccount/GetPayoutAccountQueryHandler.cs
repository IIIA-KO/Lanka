using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.GetPayoutAccount;

internal sealed class GetPayoutAccountQueryHandler
    : IQueryHandler<GetPayoutAccountQuery, PayoutAccountResponse>
{
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUserContext _userContext;

    public GetPayoutAccountQueryHandler(
        IBloggerRepository bloggerRepository,
        IUserContext userContext)
    {
        this._bloggerRepository = bloggerRepository;
        this._userContext = userContext;
    }

    public async Task<Result<PayoutAccountResponse>> Handle(
        GetPayoutAccountQuery request,
        CancellationToken cancellationToken)
    {
        Blogger? blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );

        if (blogger is null)
        {
            return Result.Failure<PayoutAccountResponse>(BloggerErrors.NotFound);
        }

        if (blogger.PayoutAccount is null)
        {
            return Result.Failure<PayoutAccountResponse>(
                Error.NotFound("Blogger.PayoutAccountNotFound", "No payout account configured.")
            );
        }

        return new PayoutAccountResponse(
            blogger.PayoutAccount.Iban.Masked,
            blogger.PayoutAccount.Currency
        );
    }
}
