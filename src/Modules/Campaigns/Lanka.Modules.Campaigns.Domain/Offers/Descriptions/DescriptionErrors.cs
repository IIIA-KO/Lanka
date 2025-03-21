using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Offers.Descriptions;

public static class DescriptionErrors
{
    public static readonly Error Empty =
        Error.Validation("Description.Empty", "Description cannot be empty.");

    public static Error TooLong(int maxLength) =>
        Error.Validation("Description.TooLong", $"Description cannot exceed {maxLength}.");
}
