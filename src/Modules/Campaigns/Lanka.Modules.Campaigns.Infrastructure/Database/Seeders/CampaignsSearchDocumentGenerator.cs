using System.Data.Common;
using System.Globalization;
using Dapper;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Index;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Serilog;

namespace Lanka.Modules.Campaigns.Infrastructure.Database.Seeders;

/// <summary>
/// Generates Elasticsearch SearchDocuments for Campaigns module entities.
/// This keeps module boundaries intact - Campaigns module only accesses its own database schema.
/// </summary>
public static class CampaignsSearchDocumentGenerator
{
    public static async Task<List<SearchDocument>> GenerateAllDocumentsAsync(
        DbConnection connection,
        List<Guid> bloggerIds,
        List<Guid> pactIds,
        List<Guid> offerIds,
        List<Guid> campaignIds,
        List<Guid> reviewIds
    )
    {
        ArgumentNullException.ThrowIfNull(connection);

        Log.Information("Generating Campaigns module search documents");

        var allDocuments = new List<SearchDocument>();

        allDocuments.AddRange(await GenerateBloggerDocumentsAsync(connection, bloggerIds));
        allDocuments.AddRange(await GeneratePactDocumentsAsync(connection, pactIds));
        allDocuments.AddRange(await GenerateOfferDocumentsAsync(connection, offerIds));
        allDocuments.AddRange(await GenerateCampaignDocumentsAsync(connection, campaignIds));
        allDocuments.AddRange(await GenerateReviewDocumentsAsync(connection, reviewIds));

        Log.Information("Generated {Count} search documents from Campaigns module", allDocuments.Count);

        return allDocuments;
    }

    private static async Task<List<SearchDocument>> GenerateBloggerDocumentsAsync(
        DbConnection connection,
        List<Guid> bloggerIds
    )
    {
        const string sql = """
            SELECT
                b.id,
                b.first_name,
                b.last_name,
                b.email,
                b.birth_date,
                b.bio,
                b.category_name,
                b.instagram_metadata_username,
                b.instagram_metadata_followers_count,
                b.instagram_metadata_media_count
            FROM campaigns.bloggers b
            WHERE b.id = ANY(@BloggerIds)
            """;

        var commandDefinition = new CommandDefinition(sql, new { BloggerIds = bloggerIds.ToArray() });
        List<BloggerDataDto> bloggers = (await connection.QueryAsync<BloggerDataDto>(commandDefinition)).ToList();

        var documents = new List<SearchDocument>();

        foreach (BloggerDataDto blogger in bloggers)
        {
            string title = $"{blogger.FirstName} {blogger.LastName}";
            string content = $"{blogger.Bio} | Category: {blogger.CategoryName}";

            if (blogger.InstagramMetadataUsername is not null)
            {
                content += $" | Instagram: @{blogger.InstagramMetadataUsername}";
            }

            if (blogger.InstagramMetadataFollowersCount.HasValue)
            {
                content += $" | Followers: {blogger.InstagramMetadataFollowersCount}";
            }

            var tags = new List<string> { blogger.CategoryName };
            if (blogger.InstagramMetadataUsername is not null)
            {
                tags.Add("Instagram");
            }

            var metadata = new Dictionary<string, object>
            {
                ["email"] = blogger.Email,
                ["birthDate"] = blogger.BirthDate.ToString("O", CultureInfo.InvariantCulture),
                ["category"] = blogger.CategoryName
            };

            if (blogger.InstagramMetadataUsername is not null)
            {
                metadata["instagramUsername"] = blogger.InstagramMetadataUsername;
            }

            if (blogger.InstagramMetadataFollowersCount.HasValue)
            {
                metadata["followersCount"] = blogger.InstagramMetadataFollowersCount.Value;
            }

            if (blogger.InstagramMetadataMediaCount.HasValue)
            {
                metadata["mediaCount"] = blogger.InstagramMetadataMediaCount.Value;
            }

            Result<SearchDocument> result = SearchDocument.Create(
                sourceEntityId: blogger.Id,
                type: SearchableItemType.Blogger,
                title: title,
                content: content,
                tags: tags,
                metadata: metadata
            );

            if (result.IsSuccess)
            {
                documents.Add(result.Value);
            }
            else
            {
                Log.Warning("Failed to create search document for blogger {BloggerId}: {Error}",
                    blogger.Id, result.Error.Description);
            }
        }

        return documents;
    }

    private static async Task<List<SearchDocument>> GeneratePactDocumentsAsync(
        DbConnection connection,
        List<Guid> pactIds
    )
    {
        const string sql = """
            SELECT
                p.id,
                p.content,
                p.blogger_id,
                p.last_updated_on_utc,
                b.first_name,
                b.last_name
            FROM campaigns.pacts p
            INNER JOIN campaigns.bloggers b ON p.blogger_id = b.id
            WHERE p.id = ANY(@PactIds)
            """;

        var commandDefinition = new CommandDefinition(sql, new { PactIds = pactIds.ToArray() });
        List<PactDataDto> pacts = (await connection.QueryAsync<PactDataDto>(commandDefinition)).ToList();

        var documents = new List<SearchDocument>();

        foreach (PactDataDto pact in pacts)
        {
            string bloggerName = $"{pact.FirstName} {pact.LastName}";
            string title = $"{bloggerName}'s Work Agreement";
            string content = pact.Content;

            var tags = new List<string> { "Pact", "Agreement" };

            var metadata = new Dictionary<string, object>
            {
                ["bloggerId"] = pact.BloggerId,
                ["bloggerName"] = bloggerName,
                ["lastUpdatedOnUtc"] = pact.LastUpdatedOnUtc.ToString("O")
            };

            Result<SearchDocument> result = SearchDocument.Create(
                sourceEntityId: pact.Id,
                type: SearchableItemType.Pact,
                title: title,
                content: content,
                tags: tags,
                metadata: metadata
            );

            if (result.IsSuccess)
            {
                documents.Add(result.Value);
            }
            else
            {
                Log.Warning("Failed to create search document for pact {PactId}: {Error}",
                    pact.Id, result.Error.Description);
            }
        }

        return documents;
    }

    private static async Task<List<SearchDocument>> GenerateOfferDocumentsAsync(
        DbConnection connection,
        List<Guid> offerIds
    )
    {
        const string sql = """
            SELECT
                o.id,
                o.name,
                o.description,
                o.price_amount,
                o.price_currency,
                o.pact_id,
                o.last_cooperated_on_utc,
                p.blogger_id,
                b.first_name,
                b.last_name
            FROM campaigns.offers o
            INNER JOIN campaigns.pacts p ON o.pact_id = p.id
            INNER JOIN campaigns.bloggers b ON p.blogger_id = b.id
            WHERE o.id = ANY(@OfferIds)
            """;

        var commandDefinition = new CommandDefinition(sql, new { OfferIds = offerIds.ToArray() });
        List<OfferDataDto> offers = (await connection.QueryAsync<OfferDataDto>(commandDefinition)).ToList();

        var documents = new List<SearchDocument>();

        foreach (OfferDataDto offer in offers)
        {
            string bloggerName = $"{offer.FirstName} {offer.LastName}";
            string title = $"{offer.Name} by {bloggerName}";
            string content = $"{offer.Description} | Price: {offer.PriceAmount} {offer.PriceCurrency}";

            var tags = new List<string> { offer.Name, offer.PriceCurrency };

            var metadata = new Dictionary<string, object>
            {
                ["offerName"] = offer.Name,
                ["price"] = offer.PriceAmount,
                ["currency"] = offer.PriceCurrency,
                ["pactId"] = offer.PactId,
                ["bloggerId"] = offer.BloggerId,
                ["bloggerName"] = bloggerName
            };

            if (offer.LastCooperatedOnUtc.HasValue)
            {
                metadata["lastCooperatedOnUtc"] = offer.LastCooperatedOnUtc.Value.ToString("O");
            }

            Result<SearchDocument> result = SearchDocument.Create(
                sourceEntityId: offer.Id,
                type: SearchableItemType.Offer,
                title: title,
                content: content,
                tags: tags,
                metadata: metadata
            );

            if (result.IsSuccess)
            {
                documents.Add(result.Value);
            }
            else
            {
                Log.Warning("Failed to create search document for offer {OfferId}: {Error}",
                    offer.Id, result.Error.Description);
            }
        }

        return documents;
    }

    private static async Task<List<SearchDocument>> GenerateCampaignDocumentsAsync(
        DbConnection connection,
        List<Guid> campaignIds
    )
    {
        const string sql = """
            SELECT
                c.id,
                c.name,
                c.description,
                c.status,
                c.price_amount,
                c.price_currency,
                c.scheduled_on_utc,
                c.creator_id,
                c.client_id,
                c.offer_id,
                creator.first_name AS creator_first_name,
                creator.last_name AS creator_last_name,
                client.first_name AS client_first_name,
                client.last_name AS client_last_name
            FROM campaigns.campaigns c
            INNER JOIN campaigns.bloggers creator ON c.creator_id = creator.id
            INNER JOIN campaigns.bloggers client ON c.client_id = client.id
            WHERE c.id = ANY(@CampaignIds)
            """;

        var commandDefinition = new CommandDefinition(sql, new { CampaignIds = campaignIds.ToArray() });
        List<CampaignDataDto> campaigns = (await connection.QueryAsync<CampaignDataDto>(commandDefinition)).ToList();

        var documents = new List<SearchDocument>();

        foreach (CampaignDataDto campaign in campaigns)
        {
            string creatorName = $"{campaign.CreatorFirstName} {campaign.CreatorLastName}";
            string clientName = $"{campaign.ClientFirstName} {campaign.ClientLastName}";

            string title = campaign.Name;
            string content = $"{campaign.Description} | Creator: {creatorName} | Client: {clientName} | Price: {campaign.PriceAmount} {campaign.PriceCurrency}";

            string statusName = GetCampaignStatusName(campaign.Status);
            var tags = new List<string> { statusName };

            var metadata = new Dictionary<string, object>
            {
                ["status"] = campaign.Status,
                ["statusName"] = statusName,
                ["price"] = campaign.PriceAmount,
                ["currency"] = campaign.PriceCurrency,
                ["scheduledOnUtc"] = campaign.ScheduledOnUtc.ToString("O"),
                ["creatorId"] = campaign.CreatorId,
                ["creatorName"] = creatorName,
                ["clientId"] = campaign.ClientId,
                ["clientName"] = clientName,
                ["offerId"] = campaign.OfferId
            };

            Result<SearchDocument> result = SearchDocument.Create(
                sourceEntityId: campaign.Id,
                type: SearchableItemType.Campaign,
                title: title,
                content: content,
                tags: tags,
                metadata: metadata
            );

            if (result.IsSuccess)
            {
                documents.Add(result.Value);
            }
            else
            {
                Log.Warning("Failed to create search document for campaign {CampaignId}: {Error}",
                    campaign.Id, result.Error.Description);
            }
        }

        return documents;
    }

    private static async Task<List<SearchDocument>> GenerateReviewDocumentsAsync(
        DbConnection connection,
        List<Guid> reviewIds
    )
    {
        const string sql = """
            SELECT
                r.id,
                r.campaign_id,
                r.rating,
                r.comment,
                r.created_on_utc,
                c.name AS campaign_name
            FROM campaigns.reviews r
            INNER JOIN campaigns.campaigns c ON r.campaign_id = c.id
            WHERE r.id = ANY(@ReviewIds)
            """;

        var commandDefinition = new CommandDefinition(sql, new { ReviewIds = reviewIds.ToArray() });
        List<ReviewDataDto> reviews = (await connection.QueryAsync<ReviewDataDto>(commandDefinition)).ToList();

        var documents = new List<SearchDocument>();

        foreach (ReviewDataDto review in reviews)
        {
            string title = $"Review for {review.CampaignName}";
            string content = $"{review.Comment} | Rating: {review.Rating}/5";

            var tags = new List<string> { $"{review.Rating} stars" };

            var metadata = new Dictionary<string, object>
            {
                ["campaignId"] = review.CampaignId,
                ["campaignName"] = review.CampaignName,
                ["rating"] = review.Rating,
                ["createdOnUtc"] = review.CreatedOnUtc.ToString("O")
            };

            Result<SearchDocument> result = SearchDocument.Create(
                sourceEntityId: review.Id,
                type: SearchableItemType.Review,
                title: title,
                content: content,
                tags: tags,
                metadata: metadata
            );

            if (result.IsSuccess)
            {
                documents.Add(result.Value);
            }
            else
            {
                Log.Warning("Failed to create search document for review {ReviewId}: {Error}",
                    review.Id, result.Error.Description);
            }
        }

        return documents;
    }

    private static string GetCampaignStatusName(int status)
    {
        return status switch
        {
            0 => "Pending",
            1 => "Confirmed",
            2 => "Rejected",
            3 => "Done",
            4 => "Completed",
            5 => "Cancelled",
            _ => "Unknown"
        };
    }

    private sealed class BloggerDataDto
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateOnly BirthDate { get; init; }
        public string Bio { get; init; } = string.Empty;
        public string CategoryName { get; init; } = string.Empty;
        public string? InstagramMetadataUsername { get; init; }
        public int? InstagramMetadataFollowersCount { get; init; }
        public int? InstagramMetadataMediaCount { get; init; }
    }

    private sealed class PactDataDto
    {
        public Guid Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public Guid BloggerId { get; init; }
        public DateTimeOffset LastUpdatedOnUtc { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
    }

    private sealed class OfferDataDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal PriceAmount { get; init; }
        public string PriceCurrency { get; init; } = string.Empty;
        public Guid PactId { get; init; }
        public DateTimeOffset? LastCooperatedOnUtc { get; init; }
        public Guid BloggerId { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
    }

    private sealed class CampaignDataDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int Status { get; init; }
        public decimal PriceAmount { get; init; }
        public string PriceCurrency { get; init; } = string.Empty;
        public DateTimeOffset ScheduledOnUtc { get; init; }
        public Guid CreatorId { get; init; }
        public Guid ClientId { get; init; }
        public Guid OfferId { get; init; }
        public string CreatorFirstName { get; init; } = string.Empty;
        public string CreatorLastName { get; init; } = string.Empty;
        public string ClientFirstName { get; init; } = string.Empty;
        public string ClientLastName { get; init; } = string.Empty;
    }

    private sealed class ReviewDataDto
    {
        public Guid Id { get; init; }
        public Guid CampaignId { get; init; }
        public int Rating { get; init; }
        public string Comment { get; init; } = string.Empty;
        public DateTimeOffset CreatedOnUtc { get; init; }
        public string CampaignName { get; init; } = string.Empty;
    }
}
