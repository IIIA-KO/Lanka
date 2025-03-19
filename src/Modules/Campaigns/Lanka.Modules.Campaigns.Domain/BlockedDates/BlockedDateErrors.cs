using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.BlockedDates
{
    public static class BlockedDateErrors
    {
        public static readonly Error NotFound =
            Error.NotFound(
                "BlockedDate.NotFound", 
                "The blocked date with specified id was not found"
            );

        public static readonly Error AlreadyBlocked =
            Error.Conflict(
                "BlockedDate.AlreadyBlocked",
                "Date is already a blocked"
            );

        public static readonly Error PastDate =
            Error.Problem(
                "BlockedDate.PastDate",
                "The date has already passed"
            );
    }
}
