using Dapper;
using Lanka.Common.Application.Caching;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Data;
using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Notifications;
using Lanka.Common.Infrastructure.Authentication;
using Lanka.Common.Infrastructure.Authorization;
using Lanka.Common.Infrastructure.Caching;
using Lanka.Common.Infrastructure.Clock;
using Lanka.Common.Infrastructure.Data;
using Lanka.Common.Infrastructure.Notifications;
using Lanka.Common.Infrastructure.Outbox;
using MassTransit;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Quartz;
using Quartz.Simpl;
using StackExchange.Redis;

namespace Lanka.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers
    )
    {
        services.AddAuthenticationInternal();

        services.AddAuthorizationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        AddPersistence(services, configuration.GetConnectionString("Database")!);

        AddCache(services);

        AddBackgroundJobs(services);

        AddEventBus(services, serviceName, moduleConfigureConsumers, configuration.GetConnectionString("Queue")!);

        AddNotifications(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, string databaseConnectionString)
    {
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(
            databaseConnectionString
        ).Build();
        services.TryAddSingleton(dataSource);

        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    private static void AddCache(IServiceCollection services)
    {
        services.AddOptions<RedisCacheOptions>()
            .Configure<IConnectionMultiplexer>((options, multiplexer) =>
                options.ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer));

        services.AddStackExchangeRedisCache(_ => { });

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 50 * 1024 * 1024;
            options.MaximumKeyLength = 256;
            options.DefaultEntryOptions = CacheOptions.CreateHybrid();
        });

        services.TryAddSingleton<ICacheService, CacheService>();
    }

    private static void AddBackgroundJobs(IServiceCollection services)
    {
        services.Configure<QuartzOptions>(options =>
        {
            options.Scheduling.IgnoreDuplicates = true;
            options.Scheduling.OverWriteExistingData = true;
        });

        services.AddQuartz(configurator =>
        {
            var scheduler = Guid.NewGuid();

            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";

            configurator.UseJobFactory<MicrosoftDependencyInjectionJobFactory>();

            configurator.UseDefaultThreadPool(tp =>
                tp.MaxConcurrency = Environment.ProcessorCount * 2
            );
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    }

    private static void AddEventBus(
        IServiceCollection services,
        string serviceName,
        Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
        string rabbitMqConnectionString
    )
    {
        services.TryAddSingleton<IEventBus, EventBus.EventBus>();
        services.AddMassTransit(configure =>
        {
#pragma warning disable CA1308 // Replace the call to 'ToLowerInvariant' with 'ToUpperInvariant'
            string instanceId = serviceName.ToLowerInvariant().Replace('.', '-');
#pragma warning restore CA1308

            foreach (Action<IRegistrationConfigurator, string> configureConsumer in moduleConfigureConsumers)
            {
                configureConsumer(configure, instanceId);
            }

            configure.SetKebabCaseEndpointNameFormatter();

            configure.AddMessageScheduler(new Uri("queue:quartz-scheduler"));

            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqConnectionString));

                cfg.UseMessageScheduler(new Uri("queue:quartz-scheduler"));

                cfg.ConfigureEndpoints(context);
            });
        });
    }

    private static void AddNotifications(IServiceCollection services)
    {
        services.TryAddScoped<INotificationService, SignalRNotificationService>();
    }
}
