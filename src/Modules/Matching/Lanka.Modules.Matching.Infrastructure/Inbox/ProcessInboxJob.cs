using System.Data;
using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Infrastructure.Inbox;
using Lanka.Common.Infrastructure.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;

namespace Lanka.Modules.Matching.Infrastructure.Inbox;

[DisallowConcurrentExecution]
internal sealed class ProcessInboxJob : IJob
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IOptions<InboxOptions> _inboxOptions;
    private readonly ILogger<ProcessInboxJob> _logger;

    public ProcessInboxJob(IDbConnectionFactory dbConnectionFactory,
        IServiceScopeFactory serviceScopeFactory,
        IDateTimeProvider dateTimeProvider,
        IOptions<InboxOptions> inboxOptions,
        ILogger<ProcessInboxJob> logger)
    {
        this._dbConnectionFactory = dbConnectionFactory;
        this._serviceScopeFactory = serviceScopeFactory;
        this._dateTimeProvider = dateTimeProvider;
        this._inboxOptions = inboxOptions;
        this._logger = logger;
    }

    private const string ModuleName = "Matching";

    public async Task Execute(IJobExecutionContext context)
    {
        this._logger.LogInformation("{Module} - Beginning to process inbox messages", ModuleName);

        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        IReadOnlyList<InboxMessageResponse> inboxMessages = await this.GetInboxMessagesAsync(connection, transaction);

        foreach (InboxMessageResponse inboxMessage in inboxMessages)
        {
            Exception? exception = null;

            try
            {
                IIntegrationEvent integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
                    inboxMessage.Content,
                    SerializerSettings.Instance
                )!;

                using IServiceScope scope = this._serviceScopeFactory.CreateScope();

                IEnumerable<IIntegrationEventHandler> handlers = IntegrationEventHandlersFactory.GetHandlers(
                    integrationEvent.GetType(),
                    scope.ServiceProvider,
                    Presentation.AssemblyReference.Assembly
                );

                foreach (IIntegrationEventHandler integrationEventHandler in handlers)
                {
                    await integrationEventHandler.Handle(integrationEvent, context.CancellationToken);
                }
            }
            catch (Exception caughtException)
            {
                this._logger.LogError(
                    caughtException,
                    "{Module} - Exception while processing inbox message {MessageId}",
                    ModuleName,
                    inboxMessage.Id);

                exception = caughtException;
            }

            await this.UpdateInboxMessageAsync(connection, transaction, inboxMessage, exception);
        }

        await transaction.CommitAsync();

        this._logger.LogInformation("{Module} - Completed processing inbox messages", ModuleName);
    }

    private async Task<IReadOnlyList<InboxMessageResponse>> GetInboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        string sql =
            $"""
             SELECT
                id AS {nameof(InboxMessageResponse.Id)},
                content AS {nameof(InboxMessageResponse.Content)}
             FROM matching.inbox_messages
             WHERE processed_on_utc IS NULL
             ORDER BY occurred_on_utc
             LIMIT {this._inboxOptions.Value.BatchSize}
             FOR UPDATE
             """;

        IEnumerable<InboxMessageResponse> inboxMessages = await connection.QueryAsync<InboxMessageResponse>(
            sql,
            transaction: transaction
        );

        return inboxMessages.AsList();
    }

    private async Task UpdateInboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        InboxMessageResponse inboxMessage,
        Exception? exception)
    {
        const string sql =
            """
            UPDATE matching.inbox_messages
            SET processed_on_utc = @ProcessedOnUtc,
                error = @Error
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                inboxMessage.Id,
                ProcessedOnUtc = this._dateTimeProvider.UtcNow,
                Error = exception?.ToString()
            },
            transaction: transaction
        );
    }

    internal sealed record InboxMessageResponse(Guid Id, string Content);
}
