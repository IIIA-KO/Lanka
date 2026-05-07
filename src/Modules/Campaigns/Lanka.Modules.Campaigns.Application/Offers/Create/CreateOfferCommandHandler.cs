using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Offers.Create;

internal sealed class CreateOfferCommandHandler
    : ICommandHandler<CreateOfferCommand, Guid>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPactRepository _pactRepository;
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOfferCommandHandler(
        IOfferRepository offerRepository,
        IPactRepository pactRepository,
        IBloggerRepository bloggerRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        this._offerRepository = offerRepository;
        this._pactRepository = pactRepository;
        this._bloggerRepository = bloggerRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        var bloggerId = new BloggerId(this._userContext.GetUserId());

        Blogger? blogger = await this._bloggerRepository.GetByIdAsync(bloggerId, cancellationToken);
        if (blogger is null)
        {
            return Result.Failure<Guid>(BloggerErrors.NotFound);
        }

        if (blogger.PayoutAccount is null)
        {
            return Result.Failure<Guid>(BloggerErrors.PayoutAccountRequired);
        }

        if (!string.Equals(blogger.PayoutAccount.Currency, request.PriceCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<Guid>(BloggerErrors.OfferCurrencyMismatch);
        }

        Pact? pact = await this._pactRepository.GetByBloggerIdWithOffersAsync(bloggerId, cancellationToken);
        if (pact is null)
        {
            return Result.Failure<Guid>(PactErrors.NotFound);
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
            return Result.Failure<Guid>(result.Error);
        }

        Offer offer = result.Value;

        if (pact.HasOffer(offer.Name))
        {
            return Result.Failure<Guid>(OfferErrors.Duplicate);
        }

        this._offerRepository.Add(offer);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return offer.Id.Value;
    }
}
