using System.Text;
using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.Ibans;

public sealed record Iban
{
    public string Value { get; }

    public string Masked =>
        this.Value.Length > 8
            ? this.Value[..4] + new string('*', this.Value.Length - 8) + this.Value[^4..]
            : this.Value;

    private Iban(string value) => this.Value = value;

    public static Result<Iban> Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Iban>(IbanErrors.Empty);
        }

        string normalized = raw.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        if (normalized.Length is < 15 or > 34)
        {
            return Result.Failure<Iban>(IbanErrors.InvalidLength);
        }

        if (!IsValidMod97(normalized))
        {
            return Result.Failure<Iban>(IbanErrors.InvalidChecksum);
        }

        return new Iban(normalized);
    }

    private static bool IsValidMod97(string iban)
    {
        string rearranged = iban[4..] + iban[..4];

        var sb = new StringBuilder();
        foreach (char c in rearranged)
        {
            if (char.IsLetter(c))
            {
                sb.Append(c - 'A' + 10);
            }
            else
            {
                sb.Append(c);
            }
        }

        int remainder = 0;
        foreach (char c in sb.ToString())
        {
            remainder = (remainder * 10 + (c - '0')) % 97;
        }

        return remainder == 1;
    }
}
