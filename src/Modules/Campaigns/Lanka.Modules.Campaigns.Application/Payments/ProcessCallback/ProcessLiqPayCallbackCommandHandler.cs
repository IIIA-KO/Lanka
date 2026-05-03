using System.Text;
using System.Text.Json;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Payments;
using Lanka.Modules.Campaigns.Domain.Payments;

namespace Lanka.Modules.Campaigns.Application.Payments.ProcessCallback;

internal sealed class ProcessLiqPayCallbackCommandHandler
    : ICommandHandler<ProcessLiqPayCallbackCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILiqPayService _liqPayService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessLiqPayCallbackCommandHandler(
        IPaymentRepository paymentRepository,
        ILiqPayService liqPayService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork
    )
    {
        this._paymentRepository = paymentRepository;
        this._liqPayService = liqPayService;
        this._dateTimeProvider = dateTimeProvider;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ProcessLiqPayCallbackCommand request, CancellationToken cancellationToken)
    {
        if (!this._liqPayService.VerifySignature(request.Data, request.Signature))
        {
            return Result.Failure(PaymentErrors.InvalidSignature);
        }

        string json = Encoding.UTF8.GetString(Convert.FromBase64String(request.Data));
        using var doc = JsonDocument.Parse(json);

        string? orderId = doc.RootElement.TryGetProperty("order_id", out JsonElement orderIdEl)
            ? orderIdEl.GetString()
            : null;

        string? status = doc.RootElement.TryGetProperty("status", out JsonElement statusEl)
            ? statusEl.GetString()
            : null;

        if (orderId is null || status is null)
        {
            return Result.Failure(PaymentErrors.InvalidSignature);
        }

        Payment? payment = await this._paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);

        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound);
        }

        if (status == "success" || status == "sandbox")
        {
            payment.Complete(this._dateTimeProvider.UtcNow);
        }
        else
        {
            payment.Fail();
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
