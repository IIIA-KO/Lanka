using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Offers.GetOffer;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Offers.Edit;

internal sealed class EditOfferCommandHandler
    : ICommandHandler<EditOfferCommand, OfferResponse>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPactRepository _pactRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public EditOfferCommandHandler(
        IOfferRepository offerRepository,
        IPactRepository pactRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._offerRepository = offerRepository;
        this._pactRepository = pactRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<OfferResponse>> Handle(EditOfferCommand request, CancellationToken cancellationToken)
    {
        Pact? pact = await this._pactRepository.GetByBloggerIdWithOffersAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );

        if (pact is null)
        {
            return Result.Failure<OfferResponse>(PactErrors.NotFound);
        }
        
        Offer? offer = await this._offerRepository.GetByIdAsync(
            new OfferId(request.OfferId),
            cancellationToken
        );
        
        if (offer is null)
        {
            return Result.Failure<OfferResponse>(OfferErrors.NotFound);
        }
        
        bool hasActiveCampaign = await this._offerRepository.HasActiveCampaignsAsync(
            offer,
            cancellationToken
        );

        if (hasActiveCampaign)
        {
            return Result.Failure<OfferResponse>(OfferErrors.HasActiveCampaigns);
        }

        Result result = offer.Update(
            request.Name,
            request.Description,
            request.PriceAmount,
            request.PriceCurrency
        );

        if (result.IsFailure)
        {
            return Result.Failure<OfferResponse>(result.Error);
        }
        
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new OfferResponse(
            offer.Id.Value,
            request.Name,
            request.PriceAmount,
            request.PriceCurrency,
            request.Description
        );
    }
}
