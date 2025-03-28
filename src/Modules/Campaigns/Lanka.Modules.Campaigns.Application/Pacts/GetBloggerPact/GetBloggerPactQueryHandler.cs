using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Offers.GetOffer;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;

internal sealed class GetBloggerPactQueryHandler
    : IQueryHandler<GetBloggerPactQuery, PactResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetBloggerPactQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<PactResponse>> Handle(GetBloggerPactQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 p.id AS Id,
                 p.blogger_id AS {nameof(PactResponse.BloggerId)},
                 p.content AS {nameof(PactResponse.Content)},
                 1 AS SplitOn,
                 o. AS {nameof(OfferResponse.Id)},
                 o.name AS {nameof(OfferResponse.Name)},
                 o.price_amount AS {nameof(OfferResponse.PriceAmount)},
                 o.price_currency AS {nameof(OfferResponse.PriceCurrency)},
                 o.description AS {nameof(OfferResponse.Description)},
             FROM campaign.pacts p
             LEFT JOIN campaign.offers o ON p.id = o.pact_id
             WHERE p.blogger_id = @BloggerId;
             """;

        PactResponse? pactResponse = null;
        
        await connection.QueryAsync<PactResponse, OfferResponse?, PactResponse>(
            sql,
            (pact, offer) =>
            {
                pactResponse ??= new PactResponse(
                    pact.Id,
                    pact.BloggerId,
                    pact.Content,
                    []
                );

                if (offer is not null)
                {
                    pactResponse.Offers.Add(offer);
                }

                return pactResponse;
            },
            new { request.BloggerId },
            splitOn: "SplitOn"
        );

        return pactResponse ?? Result.Failure<PactResponse>(PactErrors.NotFound);
    }
}
