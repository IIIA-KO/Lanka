using System.Globalization;
using Lanka.Common.Domain;
using Lanka.Common.Infrastructure.ChangeCapture;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Infrastructure.ChangeCapture;

internal sealed class CampaignsChangeCaptureInterceptor : ChangeCaptureInterceptorBase
{
    protected override CapturedChangeData? ExtractCapturedData(IChangeCaptured entity)
    {
        return entity switch
        {
            Blogger b => new CapturedChangeData(
                nameof(Blogger),
                $"{b.FirstName.Value} {b.LastName.Value}",
                b.Bio.Value,
                ["blogger", "profile", b.Category.Name],
                new Dictionary<string, object>
                {
                    { "BirthDate", b.BirthDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                    { "Category", b.Category.Name },
                    { "InstagramUsername", b.InstagramMetadata.Username ?? string.Empty },
                    { "FollowersCount", b.InstagramMetadata.FollowersCount ?? 0 },
                    { "MediaCount", b.InstagramMetadata.MediaCount ?? 0 },
                    { "EngagementRate", b.InstagramMetadata.EngagementRate ?? 0.0 },
                    { "AudienceTopAgeGroup", b.InstagramMetadata.AudienceTopAgeGroup ?? string.Empty },
                    { "AudienceTopGender", b.InstagramMetadata.AudienceTopGender ?? string.Empty },
                    { "AudienceTopGenderPercentage", b.InstagramMetadata.AudienceTopGenderPercentage ?? 0.0 },
                    { "AudienceTopCountry", b.InstagramMetadata.AudienceTopCountry ?? string.Empty },
                    { "AudienceTopCountryPercentage", b.InstagramMetadata.AudienceTopCountryPercentage ?? 0.0 },
                }),

            Campaign c => new CapturedChangeData(
                nameof(Campaign),
                c.Name.Value,
                c.Description.Value,
                ["campaign", c.Status.ToString()],
                new Dictionary<string, object>
                {
                    { "Status", c.Status.ToString() },
                    { "PriceAmount", c.Price.Amount },
                    { "PriceCurrency", c.Price.Currency.ToString() },
                }),

            Offer o => new CapturedChangeData(
                nameof(Offer),
                o.Name.Value,
                o.Description.Value,
                ["offer"],
                new Dictionary<string, object>
                {
                    { "PriceAmount", o.Price.Amount },
                    { "PriceCurrency", o.Price.Currency.ToString() },
                }),

            Pact p => new CapturedChangeData(
                nameof(Pact),
                "Pact",
                p.Content.Value,
                ["pact"]),

            Review r => new CapturedChangeData(
                nameof(Review),
                $"Review ({r.Rating.Value}/5)",
                r.Comment.Value,
                ["review"],
                new Dictionary<string, object>
                {
                    { "Rating", r.Rating.Value },
                }),

            _ => null,
        };
    }

    protected override string? GetItemType(IChangeCaptured entity)
    {
        return entity switch
        {
            Blogger => nameof(Blogger),
            Campaign => nameof(Campaign),
            Offer => nameof(Offer),
            Pact => nameof(Pact),
            Review => nameof(Review),
            _ => null,
        };
    }
}
