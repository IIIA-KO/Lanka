using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Chat;
using Lanka.Modules.Campaigns.Domain.Chat;

namespace Lanka.Modules.Campaigns.Application.Chat.GetMessages;

internal sealed class GetChatMessagesQueryHandler
    : IQueryHandler<GetChatMessagesQuery, IReadOnlyList<ChatMessageResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IChatThreadRepository _chatThreadRepository;
    private readonly IUserContext _userContext;

    public GetChatMessagesQueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        IChatThreadRepository chatThreadRepository,
        IUserContext userContext)
    {
        this._dbConnectionFactory = dbConnectionFactory;
        this._chatThreadRepository = chatThreadRepository;
        this._userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<ChatMessageResponse>>> Handle(
        GetChatMessagesQuery request,
        CancellationToken cancellationToken)
    {
        ChatThread? thread = await this._chatThreadRepository.GetByIdAsync(
            new ChatThreadId(request.ThreadId),
            cancellationToken);

        if (thread is null)
        {
            return Result.Failure<IReadOnlyList<ChatMessageResponse>>(ChatThreadErrors.NotFound);
        }

        if (!ChatAuthorization.IsParticipant(thread, this._userContext.GetUserId()))
        {
            return Result.Failure<IReadOnlyList<ChatMessageResponse>>(Error.NotAuthorized);
        }

        int limit = Math.Clamp(request.Limit, 1, 100);

        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 cm.id AS {nameof(ChatMessageResponse.Id)},
                 cm.thread_id AS {nameof(ChatMessageResponse.ThreadId)},
                 cm.sender_blogger_id AS {nameof(ChatMessageResponse.SenderBloggerId)},
                 COALESCE(b.first_name, '') AS {nameof(ChatMessageResponse.SenderFirstName)},
                 COALESCE(b.last_name, '') AS {nameof(ChatMessageResponse.SenderLastName)},
                 cm.content AS {nameof(ChatMessageResponse.Content)},
                 cm.is_system AS {nameof(ChatMessageResponse.IsSystem)},
                 cm.is_deleted AS {nameof(ChatMessageResponse.IsDeleted)},
                 cm.edited_at_utc AS {nameof(ChatMessageResponse.EditedAtUtc)},
                 cm.read_at_utc AS {nameof(ChatMessageResponse.ReadAtUtc)},
                 cm.created_at_utc AS {nameof(ChatMessageResponse.CreatedAtUtc)}
             FROM campaigns.chat_messages cm
             LEFT JOIN campaigns.bloggers b ON b.id = cm.sender_blogger_id
             WHERE cm.thread_id = @ThreadId
               AND (@Before IS NULL OR cm.created_at_utc < @Before)
             ORDER BY cm.created_at_utc DESC
             LIMIT @Limit
             """;

        List<ChatMessageResponse> messages = (await connection.QueryAsync<ChatMessageResponse>(
            sql,
            new
            {
                request.ThreadId,
                request.Before,
                Limit = limit
            })).AsList();

        return messages;
    }
}
