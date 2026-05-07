namespace Lanka.Modules.Users.Domain.Users;

public sealed class Permission
{
    public static readonly Permission ReadUsers = new("users:read");
    public static readonly Permission CreateUser = new("users:create");
    public static readonly Permission UpdateUser = new("users:update");
    public static readonly Permission DeleteUser = new("users:delete");
    
    public static readonly Permission ReadProfile = new("profile:read");
    public static readonly Permission UpdateProfile = new("profile:update");
    
    public static readonly Permission ReadBloggers = new("bloggers:read");

    public static readonly Permission ReadOffers = new("offers:read");
    public static readonly Permission CreateOffer = new("offers:create");
    public static readonly Permission UpdateOffer = new("offers:update");
    public static readonly Permission DeleteOffer = new("offers:delete");

    public static readonly Permission ReadPacts = new("pacts:read");
    public static readonly Permission CreatePact = new("pacts:create");
    public static readonly Permission UpdatePact = new("pacts:update");
    
    public static readonly Permission ReadCampaigns = new("campaigns:read");
    public static readonly Permission CreateCampaign = new("campaigns:create");
    public static readonly Permission UpdateCampaign = new("campaigns:update");
    
    public static readonly Permission ReadReviews = new("reviews:read");
    public static readonly Permission CreateReview = new("reviews:create");
    public static readonly Permission UpdateReview = new("reviews:update");
    public static readonly Permission DeleteReview = new("reviews:delete");

    public static readonly Permission ReadNotifications = new("notifications:read");
    public static readonly Permission UpdateNotifications = new("notifications:update");

    public static readonly Permission ReadPayments = new("payments:read");
    public static readonly Permission CreatePayment = new("payments:create");

    public static readonly Permission ReadChat = new("chat:read");
    public static readonly Permission WriteChat = new("chat:write");
    
    public Permission(string code)
    {
        this.Code = code;
    }

    public string Code { get; }
}
