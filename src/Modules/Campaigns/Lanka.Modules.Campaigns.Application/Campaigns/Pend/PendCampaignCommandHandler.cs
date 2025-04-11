using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Pend;

internal sealed class PendCampaignCommandHandler
    : ICommandHandler<PendCampaignCommand, CampaignId>
{
    private readonly IPactRepository _pactRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserContext _userContext;

    public PendCampaignCommandHandler(
        IPactRepository pactRepository,
        ICampaignRepository campaignRepository,
        IOfferRepository offerRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IUserContext userContext
    )
    {
        this._pactRepository = pactRepository;
        this._campaignRepository = campaignRepository;
        this._offerRepository = offerRepository;
        this._unitOfWork = unitOfWork;
        this._dateTimeProvider = dateTimeProvider;
        this._userContext = userContext;
    }

    public async Task<Result<CampaignId>> Handle(PendCampaignCommand request, CancellationToken cancellationToken)
    {
        Offer? offer = await this._offerRepository.GetByIdAsync(
            request.OfferId,
            cancellationToken
        );

        if (offer is null)
        {
            return Result.Failure<CampaignId>(OfferErrors.NotFound);
        }

        Pact? creatorPact = await this._pactRepository.GetByIdWithOffersAsync(
            offer.PactId,
            cancellationToken
        );

        if (creatorPact is null)
        {
            return Result.Failure<CampaignId>(OfferErrors.NotFound);
        }

        var clientId = new BloggerId(this._userContext.GetUserId());

        if (clientId == creatorPact.BloggerId)
        {
            return Result.Failure<CampaignId>(CampaignErrors.SameUser);
        }

        if (!creatorPact.HasOffer(offer.Name))
        {
            return Result.Failure<CampaignId>(OfferErrors.NotFound);
        }

        if (
            await this._campaignRepository.IsAlreadyStartedAsync(
                offer,
                request.ScheduledOnUtc,
                cancellationToken
            )
        )
        {
            return Result.Failure<CampaignId>(CampaignErrors.AlreadyStarted);
        }

        try
        {
            Result<Campaign> result = Campaign.Pend(
                request.Name,
                request.Description,
                request.ScheduledOnUtc,
                offer,
                clientId,
                creatorPact.BloggerId,
                this._dateTimeProvider.UtcNow
            );

            if (result.IsFailure)
            {
                return Result.Failure<CampaignId>(result.Error);
            }

            Campaign campaign = result.Value;

            this._campaignRepository.Add(campaign);

            await this._unitOfWork.SaveChangesAsync(cancellationToken);

            return campaign.Id;
        }
        catch (ConcurrencyException)
        {
            return Result.Failure<CampaignId>(CampaignErrors.AlreadyStarted);
        }
    }
}
