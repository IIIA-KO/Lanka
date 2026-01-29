using System.Globalization;
using System.Text;
using Lanka.Common.Contracts.Seeding;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.Index;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Serilog;

namespace Lanka.Modules.Matching.Infrastructure.Elasticsearch.Seeders;

public static class ElasticsearchSeeder
{
    private const int _batchSize = 1000;

    public static async Task SeedAsync(
        ISearchIndexService searchIndexService,
        List<BloggerSeedData> bloggers,
        List<InstagramAccountSeedData> igAccounts,
        List<PactSeedData> pacts,
        List<OfferSeedData> offers,
        List<CampaignSeedData> campaigns,
        List<ReviewSeedData> reviews
    )
    {
        ArgumentNullException.ThrowIfNull(searchIndexService);

        Log.Information("Starting Elasticsearch seeding");

        var allDocuments = new List<SearchDocument>();

        allDocuments.AddRange(await CreateNewBloggerDocumentsAsync(searchIndexService, bloggers));
        allDocuments.AddRange(await CreateNewInstagramAccountDocumentsAsync(searchIndexService, igAccounts));
        allDocuments.AddRange(await CreateNewPactDocumentsAsync(searchIndexService, pacts));
        allDocuments.AddRange(await CreateNewOfferDocumentsAsync(searchIndexService, offers));
        allDocuments.AddRange(await CreateNewCampaignDocumentsAsync(searchIndexService, campaigns));
        allDocuments.AddRange(await CreateNewReviewDocumentsAsync(searchIndexService, reviews));

        if (allDocuments.Count == 0)
        {
            Log.Information("All documents already exist in Elasticsearch, skipping seeding");
            return;
        }

        Log.Information("Bulk indexing {Count} new search documents", allDocuments.Count);

        for (int i = 0; i < allDocuments.Count; i += _batchSize)
        {
            var batch = allDocuments.Skip(i).Take(_batchSize).ToList();
            Result result = await searchIndexService.IndexDocumentsAsync(batch);

            if (result.IsFailure)
            {
                Log.Warning("Failed to index batch {BatchNumber}: {Error}",
                    (i / _batchSize) + 1, result.Error.Description);
            }
            else
            {
                Log.Information("Successfully indexed batch {BatchNumber} with {Count} documents",
                    (i / _batchSize) + 1, batch.Count);
            }
        }

        Log.Information("Elasticsearch seeding completed successfully");
    }

    private static async Task<List<SearchDocument>> CreateNewBloggerDocumentsAsync(
        ISearchIndexService searchIndexService,
        List<BloggerSeedData> bloggers
    )
    {
        if (bloggers.Count == 0)
        {
            return [];
        }

        HashSet<Guid> existingIds = await searchIndexService.GetExistingSourceEntityIdsAsync(
            bloggers.Select(b => b.Id),
            SearchableItemType.Blogger);

        var newBloggers = bloggers.Where(b => !existingIds.Contains(b.Id)).ToList();

        if (newBloggers.Count == 0)
        {
            Log.Information("Bloggers already indexed ({Count} exist)", bloggers.Count);
            return [];
        }

        Log.Information("Creating search documents for {Count} new bloggers (out of {Total})",
            newBloggers.Count, bloggers.Count);

        return CreateBloggerDocuments(newBloggers);
    }

    private static async Task<List<SearchDocument>> CreateNewInstagramAccountDocumentsAsync(
        ISearchIndexService searchIndexService,
        List<InstagramAccountSeedData> accounts
    )
    {
        if (accounts.Count == 0)
        {
            return [];
        }

        HashSet<Guid> existingIds = await searchIndexService.GetExistingSourceEntityIdsAsync(
            accounts.Select(a => a.Id),
            SearchableItemType.InstagramAccount);

        var newAccounts = accounts.Where(a => !existingIds.Contains(a.Id)).ToList();

        if (newAccounts.Count == 0)
        {
            Log.Information("Instagram accounts already indexed ({Count} exist)", accounts.Count);
            return [];
        }

        Log.Information("Creating search documents for {Count} new Instagram accounts (out of {Total})",
            newAccounts.Count, accounts.Count);

        return CreateInstagramAccountDocuments(newAccounts);
    }

    private static async Task<List<SearchDocument>> CreateNewPactDocumentsAsync(
        ISearchIndexService searchIndexService,
        List<PactSeedData> pacts
    )
    {
        if (pacts.Count == 0)
        {
            return [];
        }

        HashSet<Guid> existingIds = await searchIndexService.GetExistingSourceEntityIdsAsync(
            pacts.Select(p => p.Id),
            SearchableItemType.Pact);

        var newPacts = pacts.Where(p => !existingIds.Contains(p.Id)).ToList();

        if (newPacts.Count == 0)
        {
            Log.Information("Pacts already indexed ({Count} exist)", pacts.Count);
            return [];
        }

        Log.Information("Creating search documents for {Count} new pacts (out of {Total})",
            newPacts.Count, pacts.Count);

        return CreatePactDocuments(newPacts);
    }

    private static async Task<List<SearchDocument>> CreateNewOfferDocumentsAsync(
        ISearchIndexService searchIndexService,
        List<OfferSeedData> offers
    )
    {
        if (offers.Count == 0)
        {
            return [];
        }

        HashSet<Guid> existingIds = await searchIndexService.GetExistingSourceEntityIdsAsync(
            offers.Select(o => o.Id),
            SearchableItemType.Offer);

        var newOffers = offers.Where(o => !existingIds.Contains(o.Id)).ToList();

        if (newOffers.Count == 0)
        {
            Log.Information("Offers already indexed ({Count} exist)", offers.Count);
            return [];
        }

        Log.Information("Creating search documents for {Count} new offers (out of {Total})",
            newOffers.Count, offers.Count);

        return CreateOfferDocuments(newOffers);
    }

    private static async Task<List<SearchDocument>> CreateNewCampaignDocumentsAsync(
        ISearchIndexService searchIndexService,
        List<CampaignSeedData> campaigns
    )
    {
        if (campaigns.Count == 0)
        {
            return [];
        }

        HashSet<Guid> existingIds = await searchIndexService.GetExistingSourceEntityIdsAsync(
            campaigns.Select(c => c.Id),
            SearchableItemType.Campaign);

        var newCampaigns = campaigns.Where(c => !existingIds.Contains(c.Id)).ToList();

        if (newCampaigns.Count == 0)
        {
            Log.Information("Campaigns already indexed ({Count} exist)", campaigns.Count);
            return [];
        }

        Log.Information("Creating search documents for {Count} new campaigns (out of {Total})",
            newCampaigns.Count, campaigns.Count);

        return CreateCampaignDocuments(newCampaigns);
    }

    private static async Task<List<SearchDocument>> CreateNewReviewDocumentsAsync(
        ISearchIndexService searchIndexService,
        List<ReviewSeedData> reviews
    )
    {
        if (reviews.Count == 0)
        {
            return [];
        }

        HashSet<Guid> existingIds = await searchIndexService.GetExistingSourceEntityIdsAsync(
            reviews.Select(r => r.Id),
            SearchableItemType.Review);

        var newReviews = reviews.Where(r => !existingIds.Contains(r.Id)).ToList();

        if (newReviews.Count == 0)
        {
            Log.Information("Reviews already indexed ({Count} exist)", reviews.Count);
            return [];
        }

        Log.Information("Creating search documents for {Count} new reviews (out of {Total})",
            newReviews.Count, reviews.Count);

        return CreateReviewDocuments(newReviews);
    }

    private static List<SearchDocument> CreateBloggerDocuments(List<BloggerSeedData> bloggers)
    {
        var documents = new List<SearchDocument>();

        foreach (BloggerSeedData blogger in bloggers)
        {
            string title = $"{blogger.FirstName} {blogger.LastName}";
            var content = new StringBuilder($"{blogger.Bio} | Category: {blogger.CategoryName}");

            if (blogger.InstagramMetadataUsername is not null)
            {
                content.Append($" | Instagram: @").Append(blogger.InstagramMetadataUsername);
            }

            if (blogger.InstagramMetadataFollowersCount.HasValue)
            {
                content.Append($" | Followers: ").Append(blogger.InstagramMetadataFollowersCount);
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
                content: content.ToString(),
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

    private static List<SearchDocument> CreateInstagramAccountDocuments(List<InstagramAccountSeedData> accounts)
    {
        var documents = new List<SearchDocument>();

        foreach (InstagramAccountSeedData account in accounts)
        {
            string title = $"@{account.MetadataUserName}";
            string content =
                $"Instagram Account | Followers: {account.MetadataFollowersCount} | Posts: {account.MetadataMediaCount}";

            var tags = new List<string> { "Instagram", "Social Media" };

            var metadata = new Dictionary<string, object>
            {
                ["username"] = account.MetadataUserName,
                ["followersCount"] = account.MetadataFollowersCount,
                ["mediaCount"] = account.MetadataMediaCount
            };

            Result<SearchDocument> result = SearchDocument.Create(
                sourceEntityId: account.Id,
                type: SearchableItemType.InstagramAccount,
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
                Log.Warning("Failed to create search document for Instagram account {AccountId}: {Error}",
                    account.Id, result.Error.Description);
            }
        }

        return documents;
    }

    private static List<SearchDocument> CreatePactDocuments(List<PactSeedData> pacts)
    {
        var documents = new List<SearchDocument>();

        foreach (PactSeedData pact in pacts)
        {
            string bloggerName = $"{pact.BloggerFirstName} {pact.BloggerLastName}";
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

    private static List<SearchDocument> CreateOfferDocuments(List<OfferSeedData> offers)
    {
        var documents = new List<SearchDocument>();

        foreach (OfferSeedData offer in offers)
        {
            string bloggerName = $"{offer.BloggerFirstName} {offer.BloggerLastName}";
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

    private static List<SearchDocument> CreateCampaignDocuments(List<CampaignSeedData> campaigns)
    {
        var documents = new List<SearchDocument>();

        foreach (CampaignSeedData campaign in campaigns)
        {
            string creatorName = $"{campaign.CreatorFirstName} {campaign.CreatorLastName}";
            string clientName = $"{campaign.ClientFirstName} {campaign.ClientLastName}";

            string title = campaign.Name;
            string content =
                $"{campaign.Description} | Creator: {creatorName} | Client: {clientName} | Price: {campaign.PriceAmount} {campaign.PriceCurrency}";

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

    private static List<SearchDocument> CreateReviewDocuments(List<ReviewSeedData> reviews)
    {
        var documents = new List<SearchDocument>();

        foreach (ReviewSeedData review in reviews)
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
}
