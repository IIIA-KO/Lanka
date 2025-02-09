using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.Bios
{
    public static class BioErrors
    {
        public static Error TooLong(int maxLength) =>
            Error.Validation("Bio.TooLong", $"Bio cannot exceed {maxLength}.");
    }
}
