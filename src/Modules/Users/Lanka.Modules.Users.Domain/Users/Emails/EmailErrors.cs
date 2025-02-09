using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users.Emails
{
    public static class EmailErrors
    {
        public static readonly Error Empty =
            Error.Validation("Email.Empty", "Email cannot be empty.");
        
        public static Error TooLonng(int maxLength) =>
            Error.Validation("Email.TooLong", $"Email cannot exceed {maxLength}.");
    }
}
