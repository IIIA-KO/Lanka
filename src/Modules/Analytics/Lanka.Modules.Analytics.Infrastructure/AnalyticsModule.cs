using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Infrastructure.Outbox;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;
using Lanka.Modules.Analytics.Infrastructure.Database;
using Lanka.Modules.Analytics.Infrastructure.Inbox;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Services;
using Lanka.Modules.Analytics.Infrastructure.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Outbox;
using Lanka.Modules.Analytics.Infrastructure.Tokens;
using Lanka.Modules.Users.IntegrationEvents;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure;

public static class AnalyticsModule
{
    public static IServiceCollection AddAnalyticsModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDomainEventHandlers();

        services.AddIntegrationEventHandlers();

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
    {
        registrationConfigurator
            .AddConsumer<IntegrationEventConsumer<InstagramAccountLinkedIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);

        registrationConfigurator
            .AddConsumer<IntegrationEventConsumer<UserDeletedIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddInstagramIntegration(services, configuration);

        AddPersistence(services, configuration);

        AddOutbox(services, configuration);
    }

    private static void AddInstagramIntegration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InstagramOptions>(configuration.GetSection("Analytics:Instagram"));

        services
            .AddRefitClient<IFacebookApi>()
            .ConfigureHttpClient(ConfigureBaseUrl);
        
        services
            .AddRefitClient<IInstagramAccountsApi>()
            .ConfigureHttpClient(ConfigureBaseUrl);
        
        services
            .AddRefitClient<IInstagramPostsApi>()
            .ConfigureHttpClient(ConfigureBaseUrl);
        
        services
            .AddRefitClient<IInstagramAudienceApi>()
            .ConfigureHttpClient(ConfigureBaseUrl);
        
        services
            .AddRefitClient<IInstagramStatisticsApi>()
            .ConfigureHttpClient(ConfigureBaseUrl);

        static void ConfigureBaseUrl(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            InstagramOptions instagramOptions = serviceProvider
                .GetRequiredService<IOptions<InstagramOptions>>()
                .Value;

            httpClient.BaseAddress = new Uri(instagramOptions.BaseUrl);
        }

        services
            .AddRefitClient<IInstagramTokenApi>()
            .ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                InstagramOptions instagramOptions = serviceProvider
                    .GetRequiredService<IOptions<InstagramOptions>>()
                    .Value;

                httpClient.BaseAddress = new Uri(instagramOptions.TokenUrl);
            });
        
        services.AddScoped<IFacebookService, FacebookService>();
        services.AddScoped<IInstagramAccountsService, InstagramAccountsService>();
        services.AddScoped<IInstagramAudienceService, InstagramAudienceService>();
        services.AddScoped<IInstagramPostService, InstagramPostService>();
        services.AddScoped<IInstagramStatisticsService, InstagramStatisticsService>();
        services.AddScoped<IInstagramTokenService, InstagramTokenTokenService>();
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Analytics))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IInstagramAccountRepository, InstagramAccountRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AnalyticsDbContext>());
    }

    private static void AddOutbox(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Users:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        services.Configure<InboxOptions>(configuration.GetSection("Users:Inbox"));
        services.ConfigureOptions<ConfigureProcessInboxJob>();
    }

    private static void AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] domainEventHandlers = Application.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToArray();

        foreach (Type domainEventHandler in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandler);

            Type domainEvent = domainEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

            services.Decorate(domainEventHandler, closedIdempotentHandler);
        }
    }

    private static void AddIntegrationEventHandlers(this IServiceCollection services)
    {
        Type[] integrationEventHandlers = Presentation.AssemblyReference.Assembly
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
