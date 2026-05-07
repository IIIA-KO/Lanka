using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Payments;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Payments;

namespace Lanka.Modules.Campaigns.Application.Payments.Initiate;

internal sealed class InitiatePaymentCommandHandler
    : ICommandHandler<InitiatePaymentCommand, PaymentCheckoutResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public InitiatePaymentCommandHandler(
        ICampaignRepository campaignRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork
    )
    {
        this._campaignRepository = campaignRepository;
        this._paymentRepository = paymentRepository;
        this._paymentGateway = paymentGateway;
        this._userContext = userContext;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentCheckoutResponse>> Handle(
        InitiatePaymentCommand request,
        CancellationToken cancellationToken
    )
    {
        Campaign? campaign = await this._campaignRepository.GetByIdAsync(
            new CampaignId(request.CampaignId),
            cancellationToken
        );

        if (campaign is null)
        {
            return Result.Failure<PaymentCheckoutResponse>(CampaignErrors.NotFound);
        }

        if (campaign.ClientId.Value != this._userContext.GetUserId())
        {
            return Result.Failure<PaymentCheckoutResponse>(Error.NotAuthorized);
        }

        if (campaign.Status != CampaignStatus.Done)
        {
            return Result.Failure<PaymentCheckoutResponse>(PaymentErrors.CampaignNotConfirmed);
        }

        if (this._paymentGateway.ConfigurationError is { } configurationError)
        {
            return Result.Failure<PaymentCheckoutResponse>(PaymentErrors.ProviderNotConfigured(configurationError));
        }

        Payment? existing = await this._paymentRepository.GetByCampaignIdAsync(
            campaign.Id,
            cancellationToken
        );

        string description = $"Payment for campaign: {campaign.Name.Value}";

        if (existing is not null)
        {
            if (existing.Status == PaymentStatus.Pending)
            {
                string retryOrderId = CreateProviderOrderId(campaign.Id);
                existing.RefreshProviderOrderId(retryOrderId);

                await this._unitOfWork.SaveChangesAsync(cancellationToken);

                return this.BuildCheckoutResponse(campaign, retryOrderId, description);
            }

            if (existing.Status == PaymentStatus.Failed)
            {
                string retryOrderId = CreateProviderOrderId(campaign.Id);
                existing.Retry(retryOrderId);

                await this._unitOfWork.SaveChangesAsync(cancellationToken);

                return this.BuildCheckoutResponse(campaign, retryOrderId, description);
            }

            return Result.Failure<PaymentCheckoutResponse>(PaymentErrors.AlreadyExists);
        }

        string orderId = CreateProviderOrderId(campaign.Id);

        var payment = Payment.Create(
            campaign.Id,
            new BloggerId(this._userContext.GetUserId()),
            campaign.Price,
            orderId,
            this._dateTimeProvider.UtcNow
        );

        this._paymentRepository.Add(payment);
        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return this.BuildCheckoutResponse(campaign, orderId, description);
    }

    private static string CreateProviderOrderId(CampaignId campaignId)
    {
        return $"lanka-{campaignId.Value:N}-{Guid.NewGuid():N}";
    }

    private PaymentCheckoutResponse BuildCheckoutResponse(
        Campaign campaign,
        string orderId,
        string description
    )
    {
        PaymentCheckoutForm form = this._paymentGateway.BuildCheckoutForm(
            new PaymentCheckoutRequest(
                campaign.Price.Amount,
                campaign.Price.Currency.ToString(),
                orderId,
                description)
        );

        return new PaymentCheckoutResponse(form.ActionUrl, form.Method, form.Fields);
    }
}
