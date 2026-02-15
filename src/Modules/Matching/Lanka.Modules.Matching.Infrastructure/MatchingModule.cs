using Elastic.Clients.Elasticsearch;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Infrastructure.Outbox;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Analytics.IntegrationEvents;
using Lanka.Modules.Campaigns.IntegrationEvents.Bloggers;
using Lanka.Modules.Campaigns.IntegrationEvents.Campaigns;
using Lanka.Modules.Campaigns.IntegrationEvents.Offers;
using Lanka.Modules.Campaigns.IntegrationEvents.Pacts;
using Lanka.Modules.Campaigns.IntegrationEvents.Reviews;
using Lanka.Modules.Matching.Application.Abstractions.Data;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Infrastructure.Database;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Client;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Services;
using Lanka.Modules.Matching.Infrastructure.Inbox;
using Lanka.Modules.Matching.Infrastructure.Outbox;
using Lanka.Modules.Matching.Presentation;
using Lanka.Modules.Matching.Presentation.IntegrationEvents.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Matching.Infrastructure;

public static class MatchingModule
{
    public static IServiceCollection AddMatchingModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddIntegrationEventHandlers();

        services.AddInfrastructure(configuration);

        services.AddEndpoints(AssemblyReference.Assembly);

        return services;
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
    {
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<BloggerSearchSyncIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);

        registrationConfigurator.AddConsumer<IntegrationEventConsumer<CampaignSearchSyncIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);

        registrationConfigurator.AddConsumer<IntegrationEventConsumer<ReviewSearchSyncIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);

        registrationConfigurator.AddConsumer<IntegrationEventConsumer<OfferSearchSyncIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);

        registrationConfigurator.AddConsumer<IntegrationEventConsumer<PactSearchSyncIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);

        registrationConfigurator.AddConsumer<IntegrationEventConsumer<InstagramAccountSearchSyncIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);

        AddOutbox(services, configuration);

        AddSearch(services, configuration);
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MatchingDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Matching))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MatchingDbContext>());
    }

    private static void AddOutbox(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Matching:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        services.Configure<InboxOptions>(configuration.GetSection("Matching:Inbox"));
        services.ConfigureOptions<ConfigureProcessInboxJob>();
    }

    private static void AddSearch(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticSearchOptions>(configuration.GetSection("Matching:ElasticSearch"));

        services.AddSingleton<ElasticsearchClient>(sp =>
        {
            ElasticSearchOptions elasticSearchOptions = sp.GetRequiredService<IOptions<ElasticSearchOptions>>()
                .Value;

            string connectionString = configuration.GetConnectionString("lanka-search")!;

            ElasticsearchClientSettings settings =
                new ElasticsearchClientSettings(new Uri(connectionString))
                    .DefaultIndex(elasticSearchOptions.DefaultIndex)
                    .EnableDebugMode();

            return new ElasticsearchClient(settings);
        });

        services.AddScoped<ISearchService, ElasticSearchService>();
        services.AddScoped<ISearchIndexService, ElasticSearchIndexService>();

        services.AddHostedService<ElasticSearchInitializationService>();
    }

    private static void AddIntegrationEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<SearchSyncIntegrationEventService>();

        Type[] integrationEventHandlers = AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToArray();

        foreach (Type integrationEventHandler in integrationEventHandlers)
        {
            services.TryAddScoped(integrationEventHandler);

            Type integrationEvent = integrationEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler =
                typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

            services.Decorate(integrationEventHandler, closedIdempotentHandler);
        }
    }
}
