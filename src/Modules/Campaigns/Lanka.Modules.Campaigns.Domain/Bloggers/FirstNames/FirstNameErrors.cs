using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.FirstNames;

public static class FirstNameErrors
{
    public static readonly Error Empty =
        Error.Validation("FirstName.Empty", "First name cannot be empty.");

    public static Error TooLong(int maxLength) =>
        Error.Validation("FirstName.TooLong", $"First name cannot exceed {maxLength}.");
}
