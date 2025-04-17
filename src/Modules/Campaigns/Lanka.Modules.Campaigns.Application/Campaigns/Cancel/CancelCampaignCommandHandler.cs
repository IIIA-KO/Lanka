using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Cancel;

internal sealed class CancelCampaignCommandHandler
    : ICommandHandler<CancelCampaignCommand>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CancelCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork
    )
    {
        this._campaignRepository = campaignRepository;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelCampaignCommand request, CancellationToken cancellationToken)
    {
        Campaign? campaign = await this._campaignRepository.GetByIdAsync(
            new CampaignId(request.CampaignId), 
            cancellationToken
        );

        if (campaign is null)
        {
            return Result.Failure(CampaignErrors.NotFound);
        }
            
        Result result = campaign.Cancel(this._dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }
            
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
            
        return Result.Success();
    }
}
