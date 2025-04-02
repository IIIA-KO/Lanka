using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Confirm;

internal sealed class ConfirmCampaignCommandHandler
    : ICommandHandler<ConfirmCampaignCommand>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork
    )
    {
        this._campaignRepository = campaignRepository;
        this._userContext = userContext;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ConfirmCampaignCommand request, CancellationToken cancellationToken)
    {
        Campaign? campaign = await this._campaignRepository.GetByIdAsync(
            request.CampaignId, 
            cancellationToken
        );

        if (campaign is null)
        {
            return Result.Failure(CampaignErrors.NotFound);
        }

        if (campaign.ScheduledOnUtc <= this._dateTimeProvider.UtcNow)
        {
            return Result.Failure(CampaignErrors.InvalidTime);
        }

        if (campaign.CreatorId.Value != this._userContext.GetUserId())
        {
            return Result.Failure(Error.NotAuthorized);
        }
            
        Result result = campaign.Confirm(this._dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }
            
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
            
        return Result.Success();
    }
}
