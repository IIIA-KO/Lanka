using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.BirthDates;

public static class BirthDateErrors
{
    public static readonly Error Invalid =
        Error.Validation("BirthDate.Invalid", "The birth date is invalid.");
}
