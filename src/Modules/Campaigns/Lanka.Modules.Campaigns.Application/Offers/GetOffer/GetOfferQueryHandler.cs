using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Offers;

namespace Lanka.Modules.Campaigns.Application.Offers.GetOffer;

internal sealed class GetOfferQueryHandler
    : IQueryHandler<GetOfferQuery, OfferResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetOfferQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<OfferResponse>> Handle(GetOfferQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
            SELECT
                id AS {nameof(OfferResponse.Id)},
                name AS {nameof(OfferResponse.Name)},
                price_amount AS {nameof(OfferResponse.PriceAmount)},
                price_currency AS {nameof(OfferResponse.PriceCurrency)},
                description AS {nameof(OfferResponse.Description)}    
            FROM campaign.offers
            WHERE id = @OfferId
            """;

        OfferResponse? offer = await connection.QuerySingleOrDefaultAsync<OfferResponse>(
            sql,
            new { request.OfferId }
        );

        return offer ?? Result.Failure<OfferResponse>(OfferErrors.NotFound);
    }
}
