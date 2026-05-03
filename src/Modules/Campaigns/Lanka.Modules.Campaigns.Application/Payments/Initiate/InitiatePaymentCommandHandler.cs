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
    : ICommandHandler<InitiatePaymentCommand, LiqPayCheckoutResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILiqPayService _liqPayService;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public InitiatePaymentCommandHandler(
        ICampaignRepository campaignRepository,
        IPaymentRepository paymentRepository,
        ILiqPayService liqPayService,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork
    )
    {
        this._campaignRepository = campaignRepository;
        this._paymentRepository = paymentRepository;
        this._liqPayService = liqPayService;
        this._userContext = userContext;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<LiqPayCheckoutResponse>> Handle(
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
            return Result.Failure<LiqPayCheckoutResponse>(CampaignErrors.NotFound);
        }

        if (campaign.ClientId.Value != this._userContext.GetUserId())
        {
            return Result.Failure<LiqPayCheckoutResponse>(Error.NotAuthorized);
        }

        if (campaign.Status != CampaignStatus.Confirmed)
        {
            return Result.Failure<LiqPayCheckoutResponse>(PaymentErrors.CampaignNotConfirmed);
        }

        Payment? existing = await this._paymentRepository.GetByCampaignIdAsync(
            campaign.Id,
            cancellationToken
        );

        if (existing is not null)
        {
            return Result.Failure<LiqPayCheckoutResponse>(PaymentErrors.AlreadyExists);
        }

        string orderId = $"lanka-{campaign.Id.Value}";
        string description = $"Payment for campaign: {campaign.Name}";

        (string data, string signature) = this._liqPayService.BuildCheckoutParams(
            campaign.Price.Amount,
            campaign.Price.Currency.ToString(),
            orderId,
            description
        );

        var payment = Payment.Create(
            campaign.Id,
            new BloggerId(this._userContext.GetUserId()),
            campaign.Price,
            orderId,
            this._dateTimeProvider.UtcNow
        );

        this._paymentRepository.Add(payment);
        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return new LiqPayCheckoutResponse(data, signature);
    }
}
