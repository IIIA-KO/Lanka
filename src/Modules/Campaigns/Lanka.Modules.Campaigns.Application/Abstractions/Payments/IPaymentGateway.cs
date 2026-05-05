namespace Lanka.Modules.Campaigns.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    string? ConfigurationError { get; }

    PaymentCheckoutForm BuildCheckoutForm(PaymentCheckoutRequest request);

    PaymentCallbackResult ParseCallback(string payload);

    PaymentCallbackResponse BuildCallbackResponse(string orderId);
}
