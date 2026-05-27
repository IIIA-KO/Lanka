using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.Ibans;
using Lanka.Modules.Campaigns.Domain.Bloggers.PayoutAccounts;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Bloggers.UpdatePayoutAccount;

internal sealed class UpdatePayoutAccountCommandHandler : ICommandHandler<UpdatePayoutAccountCommand>
{
    private readonly IBloggerRepository _bloggerRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePayoutAccountCommandHandler(
        IBloggerRepository bloggerRepository,
        ICampaignRepository campaignRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        this._bloggerRepository = bloggerRepository;
        this._campaignRepository = campaignRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdatePayoutAccountCommand request, CancellationToken cancellationToken)
    {
        Blogger? blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );

        if (blogger is null)
        {
            return Result.Failure(BloggerErrors.NotFound);
        }

        Result<Iban> ibanResult = Iban.Create(request.Iban);
        if (ibanResult.IsFailure)
        {
            return Result.Failure(ibanResult.Error);
        }

        bool currencyChanging = blogger.PayoutAccount is not null
            && blogger.PayoutAccount.Currency != request.Currency;

        if (currencyChanging)
        {
            bool hasActiveCampaigns = await this._campaignRepository.HasActiveCampaignsAsync(
                new BloggerId(this._userContext.GetUserId()),
                cancellationToken
            );

            if (hasActiveCampaigns)
            {
                return Result.Failure(BloggerErrors.ActiveCampaignsExist);
            }
        }

        blogger.SetPayoutAccount(new PayoutAccount(ibanResult.Value, request.Currency));

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
