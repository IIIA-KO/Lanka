using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Common.Infrastructure.Outbox;

namespace Lanka.Modules.Matching.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent> : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly IDomainEventHandler<TDomainEvent> _decorated;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public IdempotentDomainEventHandler(
        IDomainEventHandler<TDomainEvent> decorated,
        IDbConnectionFactory dbConnectionFactory
    )
    {
        this._decorated = decorated;
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        var outboxMessageConsumer = new OutboxMessageConsumer(domainEvent.Id, this._decorated.GetType().Name);

        if (await OutboxConsumerExistsAsync(connection, outboxMessageConsumer))
        {
            return;
        }

        await this._decorated.Handle(domainEvent, cancellationToken);

        await InsertOutboxConsumerAsync(connection, outboxMessageConsumer);
    }

    private static async Task<bool> OutboxConsumerExistsAsync(
        DbConnection dbConnection,
        OutboxMessageConsumer outboxMessageConsumer)
    {
        const string sql =
            """
            SELECT EXISTS(
                SELECT 1
                FROM matching.outbox_message_consumers
                WHERE outbox_message_id = @OutboxMessageId 
                  AND name = @Name
            )
            """;

        return await dbConnection.ExecuteScalarAsync<bool>(sql, outboxMessageConsumer);
    }

    private static async Task InsertOutboxConsumerAsync(
        DbConnection dbConnection,
        OutboxMessageConsumer outboxMessageConsumer)
    {
        const string sql =
            """
            INSERT INTO users.outbox_message_consumers(outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            """;

        await dbConnection.ExecuteAsync(sql, outboxMessageConsumer);
    }
}
