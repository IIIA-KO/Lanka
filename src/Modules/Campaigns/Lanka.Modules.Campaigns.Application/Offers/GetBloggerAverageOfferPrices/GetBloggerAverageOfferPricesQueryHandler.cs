using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Offers.GetBloggerAverageOfferPrices;

internal sealed class GetBloggerAverageOfferPricesQueryHandler
    : IQueryHandler<GetBloggerAverageOfferPricesQuery, List<AveragePriceResponse>>
{
    private readonly IPactRepository _pactRepository;

    public GetBloggerAverageOfferPricesQueryHandler(IPactRepository pactRepository)
    {
        this._pactRepository = pactRepository;
    }

    public async Task<Result<List<AveragePriceResponse>>> Handle(GetBloggerAverageOfferPricesQuery request, CancellationToken cancellationToken)
    {
        Pact? pact = await this._pactRepository.GetByBloggerIdWithOffersAsync(
            new BloggerId(request.BloggerId),
            cancellationToken
        );
        
        if (pact is null)
        {
            return Result.Failure<List<AveragePriceResponse>>(PactErrors.NotFound);
        }
        
        var avaragePriceResponses = new List<AveragePriceResponse>();

        if (!pact.Offers.Any())
        {
            return avaragePriceResponses;
        }

        return pact.Offers
            .GroupBy(a => a.Price.Currency.Code)
            .Select(currencyGroup => new AveragePriceResponse(
                currencyGroup.Key.ToString(),
                currencyGroup.Average(a => a.Price.Amount)
            ))
            .ToList();
    }
}
