using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Campaigns;

public static class CampaignErrors
{
    public static readonly Error NotFound =
        Error.NotFound(
            "Campaign.NotFound",
            "The campaign with the specified identifier was not found"
        );

    public static readonly Error SameUser =
        Error.Conflict("Campaign.SameUser", "The buyer and seller cannot be the same person");

    public static readonly Error InvalidTime =
        Error.Failure("Campaign.InvalidTime", "Scheduled time cannot be in the past");

    public static readonly Error NotPending =
        Error.Failure("Campaign.NotPending", "The campaign is not pending");

    public static readonly Error NotConfirmed =
        Error.Failure("Campaign.NotConfirmed", "The campaign is not confirmed");

    public static readonly Error NotDone =
        Error.Failure("Campaign.NotDone", "The campaign is not done");
        
    public static readonly Error AlreadyStarted =
        Error.Failure("Campaign.AlreadyStarted", "The campaign has already started");
        
    public static readonly Error BlockedDate =
        Error.Failure("Campaign.BlockedDate", "The selected date is blocked for campaign");
}
