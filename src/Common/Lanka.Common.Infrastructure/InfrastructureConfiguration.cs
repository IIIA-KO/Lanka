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
using Quartz;
using StackExchange.Redis;

namespace Lanka.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<IRegistrationConfigurator>[] moduleConfigureConsumers,
        string databaseConnectionString,
        string redisConnectionString
    )
    {
        services.AddAuthenticationInternal();

        services.AddAuthorizationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.TryAddSingleton<IEventBus, EventBus.EventBus>();
        
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        
        NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(
            databaseConnectionString
        ).Build();
        services.TryAddSingleton(dataSource);

        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.AddQuartz(configurator =>
        {
            var scheduler = Guid.NewGuid();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

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

        services.AddMassTransit(configure =>
        {
            foreach (Action<IRegistrationConfigurator> configureConsumer in moduleConfigureConsumers)
            {
                configureConsumer(configure);
            }

            configure.SetKebabCaseEndpointNameFormatter();

            configure.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        return services;
    }
}
