using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.Ibans;

public static class IbanErrors
{
    public static readonly Error Empty =
        Error.Validation("Iban.Empty", "IBAN cannot be empty.");

    public static readonly Error InvalidLength =
        Error.Validation("Iban.InvalidLength", "IBAN must be between 15 and 34 characters.");

    public static readonly Error InvalidChecksum =
        Error.Validation("Iban.InvalidChecksum", "IBAN checksum is invalid. Please double-check the account number.");
}
