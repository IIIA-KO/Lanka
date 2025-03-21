using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns.Names;

public static class NameErrors
{
    public static readonly Error Empty =
        Error.Validation("Name.Empty", "Name cannot be empty.");

    public static Error TooLong(int maxLength) =>
        Error.Validation("Name.TooLong", $"Name cannot exceed {maxLength}.");
}
