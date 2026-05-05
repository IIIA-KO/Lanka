using System.Text.Json;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Payments;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Payments;

namespace Lanka.Modules.Campaigns.Application.Payments.ProcessCallback;

internal sealed class ProcessPaymentCallbackCommandHandler
    : ICommandHandler<ProcessPaymentCallbackCommand, PaymentCallbackResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentCallbackCommandHandler(
        IPaymentRepository paymentRepository,
        ICampaignRepository campaignRepository,
        IPaymentGateway paymentGateway,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        this._paymentRepository = paymentRepository;
        this._campaignRepository = campaignRepository;
        this._paymentGateway = paymentGateway;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentCallbackResponse>> Handle(
        ProcessPaymentCallbackCommand request,
        CancellationToken cancellationToken)
    {
        PaymentCallbackResult callback;
        try
        {
            callback = this._paymentGateway.ParseCallback(request.Payload);
        }
        catch (Exception ex) when (ex is InvalidOperationException or JsonException)
        {
            return Result.Failure<PaymentCallbackResponse>(PaymentErrors.InvalidSignature);
        }

        Payment? payment = await this._paymentRepository.GetByOrderIdAsync(callback.OrderId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure<PaymentCallbackResponse>(PaymentErrors.NotFound);
        }

        DateTimeOffset utcNow = this._dateTimeProvider.UtcNow;

        if (callback.IsSuccessful)
        {
            payment.Complete(utcNow);

            Campaign? campaign = await this._campaignRepository.GetByIdAsync(payment.CampaignId, cancellationToken);
            if (campaign is not null && campaign.Status == CampaignStatus.Done)
            {
                campaign.Complete(utcNow);
            }
        }
        else
        {
            payment.Fail();
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return this._paymentGateway.BuildCallbackResponse(callback.OrderId);
    }
}
