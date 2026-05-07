using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Application.Chat.GetThreads;

internal sealed class GetChatThreadsQueryHandler
    : IQueryHandler<GetChatThreadsQuery, IReadOnlyList<ChatThreadResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserContext _userContext;

    public GetChatThreadsQueryHandler(IDbConnectionFactory dbConnectionFactory, IUserContext userContext)
    {
        this._dbConnectionFactory = dbConnectionFactory;
        this._userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<ChatThreadResponse>>> Handle(
        GetChatThreadsQuery request,
        CancellationToken cancellationToken)
    {
        Guid userId = this._userContext.GetUserId();
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 ct.id AS {nameof(ChatThreadResponse.Id)},
                 other_blogger.id AS {nameof(ChatThreadResponse.OtherParticipantId)},
                 other_blogger.first_name AS {nameof(ChatThreadResponse.OtherParticipantFirstName)},
                 other_blogger.last_name AS {nameof(ChatThreadResponse.OtherParticipantLastName)},
                 other_blogger.instagram_metadata_username AS {nameof(ChatThreadResponse.OtherParticipantInstagramUsername)},
                 other_blogger.profile_photo_uri AS {nameof(ChatThreadResponse.OtherParticipantProfilePhoto)},
                 ct.campaign_id AS {nameof(ChatThreadResponse.CampaignId)},
                 c.name AS {nameof(ChatThreadResponse.CampaignName)},
                 ct.offer_id AS {nameof(ChatThreadResponse.OfferId)},
                 o.name AS {nameof(ChatThreadResponse.OfferName)},
                 last_message.content AS {nameof(ChatThreadResponse.LastMessageContent)},
                 COALESCE(last_message.is_system, false) AS {nameof(ChatThreadResponse.LastMessageIsSystem)},
                 last_message.created_at_utc AS {nameof(ChatThreadResponse.LastMessageCreatedAtUtc)},
                 COALESCE(unread.count, 0) AS {nameof(ChatThreadResponse.UnreadCount)},
                 ct.updated_at_utc AS {nameof(ChatThreadResponse.UpdatedAtUtc)}
             FROM campaigns.chat_threads ct
             INNER JOIN campaigns.bloggers other_blogger
                 ON other_blogger.id = CASE
                     WHEN ct.participant_a_id = @UserId THEN ct.participant_b_id
                     ELSE ct.participant_a_id
                 END
             LEFT JOIN campaigns.campaigns c ON c.id = ct.campaign_id
             LEFT JOIN campaigns.offers o ON o.id = ct.offer_id
             LEFT JOIN LATERAL (
                 SELECT cm.content, cm.is_system, cm.created_at_utc
                 FROM campaigns.chat_messages cm
                 WHERE cm.thread_id = ct.id
                 ORDER BY cm.created_at_utc DESC
                 LIMIT 1
             ) last_message ON true
             LEFT JOIN LATERAL (
                 SELECT COUNT(*)::int AS count
                 FROM campaigns.chat_messages cm
                 WHERE cm.thread_id = ct.id
                   AND cm.sender_blogger_id IS NOT NULL
                   AND cm.sender_blogger_id <> @UserId
                   AND cm.read_at_utc IS NULL
                   AND cm.is_deleted = false
             ) unread ON true
             WHERE ct.participant_a_id = @UserId OR ct.participant_b_id = @UserId
             ORDER BY ct.updated_at_utc DESC
             """;

        List<ChatThreadResponse> threads = (await connection.QueryAsync<ChatThreadResponse>(
            sql,
            new { UserId = userId })).AsList();

        return threads;
    }
}
