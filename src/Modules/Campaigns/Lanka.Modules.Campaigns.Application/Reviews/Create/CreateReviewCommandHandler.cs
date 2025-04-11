using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Application.Reviews.Create;

internal sealed class CreateReviewCommandHandler
    : ICommandHandler<CreateReviewCommand>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateReviewCommandHandler(
        ICampaignRepository campaignRepository,
        IReviewRepository reviewRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider
    )
    {
        this._campaignRepository = campaignRepository;
        this._reviewRepository = reviewRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
        this._dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var campaignId = new CampaignId(request.CampaignId);
        Campaign? campaign = await this._campaignRepository.GetByIdAsync(
            campaignId,
            cancellationToken
        );

        if (campaign is null)
        {
            return Result.Failure(CampaignErrors.NotFound);
        }

        if (this._userContext.GetUserId() != campaign.ClientId.Value)
        {
            return Result.Failure(Error.NotAuthorized);
        }

        Review? review = await this._reviewRepository.GetByCampaignIdAndClientIdAsync(
            campaignId,
            campaign.ClientId,
            cancellationToken
        );

        if (review is not null)
        {
            return Result.Failure(ReviewErrors.AlreadyReviewed);
        }

        Result<Review> reviewResult = Review.Create(
            campaign,
            request.Rating,
            request.Comment,
            this._dateTimeProvider.UtcNow
        );

        this._reviewRepository.Add(reviewResult.Value);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
