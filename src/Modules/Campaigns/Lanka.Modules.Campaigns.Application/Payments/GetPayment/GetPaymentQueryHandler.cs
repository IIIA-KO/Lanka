using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Payments;

namespace Lanka.Modules.Campaigns.Application.Payments.GetPayment;

internal sealed class GetPaymentQueryHandler : IQueryHandler<GetPaymentQuery, PaymentResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetPaymentQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<PaymentResponse>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 p.id AS {nameof(PaymentResponse.Id)},
                 p.campaign_id AS {nameof(PaymentResponse.CampaignId)},
                 p.amount_amount AS {nameof(PaymentResponse.Amount)},
                 p.amount_currency AS {nameof(PaymentResponse.Currency)},
                 CASE p.status
                     WHEN 1 THEN 'Pending'
                     WHEN 2 THEN 'Completed'
                     WHEN 3 THEN 'Failed'
                 END AS {nameof(PaymentResponse.Status)},
                 p.created_at_utc AS {nameof(PaymentResponse.CreatedAtUtc)},
                 p.paid_at_utc AS {nameof(PaymentResponse.PaidAtUtc)}
             FROM campaigns.payments p
             WHERE p.campaign_id = @CampaignId
             """;

        PaymentResponse? payment = await connection.QuerySingleOrDefaultAsync<PaymentResponse>(
            sql,
            new { request.CampaignId }
        );

        return payment ?? Result.Failure<PaymentResponse>(PaymentErrors.NotFound);
    }
}
