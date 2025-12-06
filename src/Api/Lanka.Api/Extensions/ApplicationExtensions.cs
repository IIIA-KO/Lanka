using HealthChecks.UI.Client;
using Lanka.Api.Middleware;
using Lanka.Api.OpenTelemetry;
using Lanka.Common.Application;
using Lanka.Common.Infrastructure;
using Lanka.Common.Infrastructure.EventBus;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Analytics.Infrastructure;
using Lanka.Modules.Campaigns.Infrastructure;
using Lanka.Modules.Matching.Infrastructure;
using Lanka.Modules.Users.Infrastructure;
using Lanka.Common.Infrastructure.Notifications;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Scalar.AspNetCore;
using Serilog;

namespace Lanka.Api.Extensions;

internal static class ApplicationExtensions
{
    public static WebApplicationBuilder ConfigureBasicServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi();

        return builder;
    }

    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(context.Configuration)
        );

        return builder;
    }

    public static WebApplicationBuilder ConfigureModules(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.AddApplication([
            Modules.Users.Application.AssemblyReference.Assembly,
            Modules.Campaigns.Application.AssemblyReference.Assembly,
            Modules.Analytics.Application.AssemblyReference.Assembly,
            Modules.Matching.Application.AssemblyReference.Assembly
        ]);

        string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
        string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;
        string mongoConnectionString = builder.Configuration.GetConnectionString("Mongo")!;
        var rabbitMqSettings = new RabbitMqSettings(builder.Configuration.GetConnectionString("Queue")!);

        builder.Services.AddInfrastructure(
            DiagnosticsConfig.ServiceName,
            [
                UsersModule.ConfigureConsumers(redisConnectionString),
                CampaignsModule.ConfigureConsumers,
                AnalyticsModule.ConfigureConsumers,
                MatchingModule.ConfigureConsumers
            ],
            rabbitMqSettings,
            databaseConnectionString,
            redisConnectionString,
            mongoConnectionString
        );

        builder.Configuration.AddModuleConfiguration(["users", "campaigns", "analytics", "matching"]);
        builder.Services.AddUsersModule(builder.Configuration);
        builder.Services.AddCampaignsModule(builder.Configuration);
        builder.Services.AddAnalyticsModule(builder.Configuration);
        builder.Services.AddMatchingModule(builder.Configuration);

        builder.Services.AddSignalR();

        return builder;
    }

    public static WebApplicationBuilder ConfigureHealthChecks(this WebApplicationBuilder builder)
    {
        string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
        string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;

        var rabbitMqSettings = new RabbitMqSettings(builder.Configuration.GetConnectionString("Queue")!);

        using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        string elasticBaseUrl = serviceProvider.GetRequiredService<IOptions<ElasticSearchOptions>>()
            .Value.BaseUrl;

        builder.Services
            .AddSingleton<IConnection>(_ =>
                {
                    var factory = new ConnectionFactory()
                    {
                        Uri = new Uri(rabbitMqSettings.Host),
                        UserName = rabbitMqSettings.Username,
                        Password = rabbitMqSettings.Password,
                    };

                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                }
            );

        builder.Services.AddHealthChecks()
            .AddNpgSql(databaseConnectionString)
            .AddRedis(redisConnectionString)
            .AddRabbitMQ()
            .AddMongoDb()
            .AddUrlGroup(
                new Uri(elasticBaseUrl),
                HttpMethod.Get,
                "elasticsearch"
            )
            .AddUrlGroup(
                new Uri(builder.Configuration.GetValue<string>("KeyCloak:HealthUrl")!),
                HttpMethod.Get,
                "keycloak"
            );

        return builder;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.ApplyMigrations();
        }

        app.UseLogContextTraceLogging();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
        app.MapHealthChecks("/healthz",
            new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
        );

        app.MapHub<InstagramHub>("/hubs/instagram");

        return app;
    }
}
