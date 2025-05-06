using Dapper;
using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Infrastructure.Authentication;
using Lanka.Common.Infrastructure.Authorization;
using Lanka.Common.Infrastructure.Caching;
using Lanka.Common.Infrastructure.Clock;
using Lanka.Common.Infrastructure.Data;
using Lanka.Common.Infrastructure.Outbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using StackExchange.Redis;

namespace Lanka.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string serviceName,
        Action<IRegistrationConfigurator>[] moduleConfigureConsumers,
        string databaseConnectionString,
        string redisConnectionString
    )
    {
        services.AddAuthenticationInternal();

        services.AddAuthorizationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        AddPersistence(services, databaseConnectionString);

        AddBackgroundJobs(services);

        AddCache(services, redisConnectionString);

        AddEventBus(services, moduleConfigureConsumers);

        AddTracing(services, serviceName);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, string databaseConnectionString)
    {
        NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(
            databaseConnectionString
        ).Build();
        services.TryAddSingleton(dataSource);

        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    private static void AddBackgroundJobs(IServiceCollection services)
    {
        services.AddQuartz(configurator =>
        {
            var scheduler = Guid.NewGuid();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    }

    private static void AddCache(IServiceCollection services, string redisConnectionString)
    {
        services.TryAddSingleton<ICacheService, CacheService>();

        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(
                redisConnectionString
            );
            services.TryAddSingleton(connectionMultiplexer);
            services.AddStackExchangeRedisCache(options =>
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer)
            );
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }
    }

    private static void AddEventBus(IServiceCollection services,
        Action<IRegistrationConfigurator>[] moduleConfigureConsumers)
    {
        services.TryAddSingleton<IEventBus, EventBus.EventBus>();
        services.AddMassTransit(configure =>
        {
            foreach (Action<IRegistrationConfigurator> configureConsumer in moduleConfigureConsumers)
            {
                configureConsumer(configure);
            }

            configure.SetKebabCaseEndpointNameFormatter();

            configure.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
    }

    private static void AddTracing(IServiceCollection services, string serviceName)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddNpgsql()
                    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);

                tracing
                    .AddOtlpExporter();
            });
    }
}
