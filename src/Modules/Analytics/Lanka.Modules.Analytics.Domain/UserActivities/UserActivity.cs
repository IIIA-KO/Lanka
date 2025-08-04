using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.UserActivities;

public sealed class UserActivity
{
    [BsonId]
    public Guid UserId { get; set; }
    
    public DateTimeOffset LastLoginAt { get; set; }

    public List<DateTimeOffset> CampaignsCompletedAsClient { get; set; } = [];

    public List<DateTimeOffset> CampaignsCompletedCreator { get; set; } = [];

    public List<ReviewActivity> ReviewsWritten { get; set; } = [];

    public double ActivityScore { get; set; }

    public string ActivityLevel { get; set; } = "Unknown";

    public static UserActivity Create(Guid userId) => new() { UserId = userId };
    
    public sealed class ReviewActivity
    {
        public Guid CampaignId { get; set; }

        public int Rating { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
