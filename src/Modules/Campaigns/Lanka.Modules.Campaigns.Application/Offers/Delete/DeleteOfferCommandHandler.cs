using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Offers.Delete;

internal sealed class DeleteOfferCommandHandler
    : ICommandHandler<DeleteOfferCommand>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPactRepository _pactRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOfferCommandHandler(
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

    public async Task<Result> Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
    {
        Pact? pact = await this._pactRepository.GetByBloggerIdWithOffersAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );
        
        if (pact is null)
        {
            return Result.Failure(PactErrors.NotFound);
        }
        
        Offer? offer = await this._offerRepository.GetByIdAsync(
            new OfferId(request.OfferId),
            cancellationToken
        );
        
        if (offer is null)
        {
            return Result.Failure(OfferErrors.NotFound);
        }
        
        bool hasActiveCampaigns = await this._offerRepository.HasActiveCampaignsAsync(
            offer,
            cancellationToken
        );

        if (hasActiveCampaigns)
        {
            return Result.Failure(OfferErrors.HasActiveCampaigns);
        }

        this._offerRepository.Remove(offer);
        
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
