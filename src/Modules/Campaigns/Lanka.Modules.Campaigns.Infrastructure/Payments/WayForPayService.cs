using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using Lanka.Modules.Campaigns.Application.Abstractions.Payments;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Campaigns.Infrastructure.Payments;

internal sealed class WayForPayService : IPaymentGateway
{
    private const string ApprovedStatus = "Approved";
    private const string ApprovedReasonCode = "1100";

    private readonly WayForPayOptions _options;

    public WayForPayService(IOptions<WayForPayOptions> options)
    {
        this._options = options.Value;
    }

    public string? ConfigurationError
    {
        get
        {
            if (IsMissing(this._options.MerchantAccount))
            {
                return "WayForPay merchant account is not configured.";
            }

            if (IsMissing(this._options.MerchantSecretKey))
            {
                return "WayForPay merchant secret key is not configured.";
            }

            if (IsMissing(this._options.MerchantDomainName))
            {
                return "WayForPay merchant domain name is not configured.";
            }

            if (!Uri.TryCreate(this._options.PaymentUrl, UriKind.Absolute, out _))
            {
                return "WayForPay payment URL is not configured.";
            }

            return null;
        }
    }

    public PaymentCheckoutForm BuildCheckoutForm(PaymentCheckoutRequest request)
    {
        string amount = FormatAmount(request.Amount);
        string orderDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        const string productCount = "1";

        string signature = this.ComputeSignature(
            this._options.MerchantAccount,
            this._options.MerchantDomainName,
            request.OrderId,
            orderDate,
            amount,
            request.Currency,
            request.Description,
            productCount,
            amount);

        var fields = new Dictionary<string, string>
        {
            ["merchantAccount"] = this._options.MerchantAccount,
            ["merchantAuthType"] = "SimpleSignature",
            ["merchantDomainName"] = this._options.MerchantDomainName,
            ["merchantTransactionType"] = "SALE",
            ["merchantTransactionSecureType"] = "AUTO",
            ["merchantSignature"] = signature,
            ["apiVersion"] = "1",
            ["orderReference"] = request.OrderId,
            ["orderDate"] = orderDate,
            ["amount"] = amount,
            ["currency"] = request.Currency,
            ["productName[]"] = request.Description,
            ["productCount[]"] = productCount,
            ["productPrice[]"] = amount,
            ["language"] = "EN"
        };

        AddPublicHttpsUrl(fields, "serviceUrl", this.ResolveProviderUrl(this._options.ServiceUrl, "/payments/wayforpay/callback"));
        AddPublicHttpsUrl(fields, "returnUrl", this.ResolveProviderUrl(this._options.ReturnUrl, "/payments/wayforpay/return"));

        return new PaymentCheckoutForm(new Uri(this._options.PaymentUrl), "POST", fields);
    }

    public PaymentCallbackResult ParseCallback(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        JsonElement root = document.RootElement;

        string orderId = GetRequiredString(root, "orderReference");
        string signature = GetRequiredString(root, "merchantSignature");
        string amount = GetRequiredString(root, "amount");
        string currency = GetRequiredString(root, "currency");
        string authCode = GetRequiredString(root, "authCode");
        string cardPan = GetRequiredString(root, "cardPan");
        string transactionStatus = GetRequiredString(root, "transactionStatus");
        string reasonCode = GetRequiredString(root, "reasonCode");

        string expectedSignature = this.ComputeSignature(
            this._options.MerchantAccount,
            orderId,
            amount,
            currency,
            authCode,
            cardPan,
            transactionStatus,
            reasonCode);

        if (!string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("WayForPay callback signature is invalid.");
        }

        return new PaymentCallbackResult(
            orderId,
            transactionStatus == ApprovedStatus && reasonCode == ApprovedReasonCode);
    }

    public PaymentCallbackResponse BuildCallbackResponse(string orderId)
    {
        const string status = "accept";
        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string signature = this.ComputeSignature(
            orderId,
            status,
            time.ToString(CultureInfo.InvariantCulture));

        return new PaymentCallbackResponse(orderId, status, time, signature);
    }

    private static bool IsMissing(string value)
    {
        return string.IsNullOrWhiteSpace(value) ||
               value.Contains('<', StringComparison.Ordinal) ||
               value.Contains('>', StringComparison.Ordinal);
    }

    private static void AddPublicHttpsUrl(Dictionary<string, string> fields, string key, string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            !uri.IsLoopback)
        {
            fields[key] = value;
        }
    }

    private string ResolveProviderUrl(string configuredUrl, string path)
    {
        if (Uri.TryCreate(this._options.PublicBaseUrl, UriKind.Absolute, out Uri? publicBaseUrl) &&
            publicBaseUrl.Scheme == Uri.UriSchemeHttps &&
            !publicBaseUrl.IsLoopback)
        {
            return new Uri(publicBaseUrl, path).ToString();
        }

        return configuredUrl;
    }

    private static string FormatAmount(decimal amount)
    {
        return amount.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string GetRequiredString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out JsonElement value))
        {
            throw new InvalidOperationException($"WayForPay callback is missing '{propertyName}'.");
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            _ => value.ToString()
        };
    }

    [SuppressMessage(
        "Security",
        "CA5351:Do Not Use Broken Cryptographic Algorithms",
        Justification = "WayForPay requires HMAC-MD5 signatures for request authentication.")]
    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "WayForPay signatures are documented as lowercase hexadecimal strings.")]
    private string ComputeSignature(params string[] values)
    {
        string source = string.Join(';', values);
        byte[] key = Encoding.UTF8.GetBytes(this._options.MerchantSecretKey);
        byte[] data = Encoding.UTF8.GetBytes(source);

        using var hmac = new HMACMD5(key);
        byte[] hash = hmac.ComputeHash(data);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
