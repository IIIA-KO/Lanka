using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.LastNames;

public static class LastNameErrors
{
    public static readonly Error Empty =
        Error.Validation("LastName.Empty", "Last name cannot be empty.");

    public static Error TooLong(int maxLength) =>
        Error.Validation("LastName.TooLong", $"Last name cannot exceed {maxLength}.");
}
