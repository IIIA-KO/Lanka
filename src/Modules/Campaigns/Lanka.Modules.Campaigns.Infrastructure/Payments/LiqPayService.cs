using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lanka.Modules.Campaigns.Application.Abstractions.Payments;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Campaigns.Infrastructure.Payments;

internal sealed class LiqPayService : ILiqPayService
{
    private readonly LiqPayOptions _options;

    public LiqPayService(IOptions<LiqPayOptions> options)
    {
        this._options = options.Value;
    }

    public (string Data, string Signature) BuildCheckoutParams(
        decimal amount,
        string currency,
        string orderId,
        string description
    )
    {
        var payload = new
        {
            version = 3,
            public_key = this._options.PublicKey,
            action = "pay",
            amount,
            currency,
            description,
            order_id = orderId,
            server_url = this._options.ServerUrl,
            result_url = this._options.ResultUrl,
            sandbox = 1
        };

        string json = JsonSerializer.Serialize(payload);
        string data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        string signature = this.ComputeSignature(data);

        return (data, signature);
    }

    public bool VerifySignature(string data, string signature)
    {
        string expected = this.ComputeSignature(data);
        return string.Equals(expected, signature, StringComparison.Ordinal);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5350", Justification = "LiqPay API requires SHA1")]
    private string ComputeSignature(string data)
    {
        string raw = this._options.PrivateKey + data + this._options.PrivateKey;
        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(hash);
    }
}
