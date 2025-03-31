using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Offers.Create;

internal sealed class CreateOfferCommandHandler
    : ICommandHandler<CreateOfferCommand, OfferId>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPactRepository _pactRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOfferCommandHandler(
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

    public async Task<Result<OfferId>> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        Pact? pact = await this._pactRepository.GetByBloggerIdWithOffersAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );

        if (pact is null)
        {
            return Result.Failure<OfferId>(PactErrors.NotFound);
        }
        
        Result<Offer> result = Offer.Create(
            pact.Id,
            request.Name,
            request.Description,
            request.PriceAmount,
            request.PriceCurrency
        );
        
        if (result.IsFailure)
        {
            return Result.Failure<OfferId>(result.Error);
        }
        
        Offer offer = result.Value;

        if (pact.HasOffer(offer.Name))
        {
            return Result.Failure<OfferId>(OfferErrors.Duplicate);
        }

        this._offerRepository.Add(offer);
        
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
        
        return offer.Id;
    }
}
