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
using Lanka.Common.Infrastructure.EventBus;
using Lanka.Common.Infrastructure.Outbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Quartz.Simpl;
using StackExchange.Redis;

namespace Lanka.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string serviceName,
        Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
        RabbitMqSettings rabbitMqSettings,
        string databaseConnectionString,
        string redisConnectionString,
        string mongoConnectionString
    )
    {
        services.AddAuthenticationInternal();

        services.AddAuthorizationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        AddPersistence(services, databaseConnectionString);

        AddCache(services, redisConnectionString);

        AddBackgroundJobs(services);

        AddEventBus(services, serviceName, moduleConfigureConsumers, rabbitMqSettings);

        AddTracing(services, serviceName, mongoConnectionString);

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

    private static void AddCache(IServiceCollection services, string redisConnectionString)
    {
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
        RabbitMqSettings rabbitMqSettings
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
                cfg.Host(new Uri(rabbitMqSettings.Host), host =>
                {
                    host.Username(rabbitMqSettings.Username);
                    host.Password(rabbitMqSettings.Password);
                });

                cfg.UseMessageScheduler(new Uri("queue:quartz-scheduler"));

                cfg.ConfigureEndpoints(context);
            });
        });
    }

    private static void AddTracing(IServiceCollection services, string serviceName, string mongoConnectionString)
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
                    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");

                tracing
                    .AddOtlpExporter();
            });

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);

            mongoClientSettings.ClusterConfigurator = c => c.Subscribe(
                new DiagnosticsActivityEventSubscriber(
                    new InstrumentationOptions
                    {
                        CaptureCommandText = true
                    }
                )
            );

            return new MongoClient(mongoClientSettings);
        });

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }
}
