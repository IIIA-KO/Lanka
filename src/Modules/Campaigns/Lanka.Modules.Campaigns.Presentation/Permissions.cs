namespace Lanka.Modules.Campaigns.Presentation;

internal static class Permissions
{
    internal const string ReadBloggers = "bloggers:read";
    
    internal const string ReadOffers = "offers:read";
    internal const string CreateOffer = "offers:create";
    internal const string UpdateOffer = "offers:update";
    internal const string DeleteOffer = "offers:delete";
    
    internal const string ReadPacts = "pacts:read";
    internal const string CreatePact = "pacts:create";
    internal const string UpdatePact = "pacts:update";
    
    internal const string ReadCampaigns = "campaigns:read";
    internal const string CreateCampaign = "campaigns:create";
    internal const string UpdateCampaign = "campaigns:update";
    
    internal const string ReadReviews = "reviews:read";
    internal const string CreateReview = "reviews:create";
    internal const string UpdateReview = "reviews:update";
    internal const string DeleteReview = "reviews:delete";
}
