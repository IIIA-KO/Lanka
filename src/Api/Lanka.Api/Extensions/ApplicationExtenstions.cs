using HealthChecks.UI.Client;
using Lanka.Api.Middleware;
using Lanka.Api.OpenTelemetry;
using Lanka.Common.Application;
using Lanka.Common.Infrastructure;
using Lanka.Common.Infrastructure.EventBus;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Campaigns.Infrastructure;
using Lanka.Modules.Users.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using Serilog;

namespace Lanka.Api.Extensions;

internal static class ApplicationExtenstions
{
    public static WebApplicationBuilder ConfigureBasicServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();

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
            Lanka.Modules.Users.Application.AssemblyReference.Assembly,
            Lanka.Modules.Campaigns.Application.AssemblyReference.Assembly
        ]);

        string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
        string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;
        var rabbitMqSettings = new RabbitMqSettings(builder.Configuration.GetConnectionString("Queue")!);

        builder.Services.AddInfrastructure(
            DiagnosticsConfig.ServiceName,
            [
                CampaignsModule.ConfigureConsumers,
                UsersModule.ConfigureConsumers
            ],
            rabbitMqSettings,
            databaseConnectionString,
            redisConnectionString
        );

        builder.Configuration.AddModuleConfiguration(["users", "campaigns"]);
        builder.Services.AddUsersModule(builder.Configuration);
        builder.Services.AddCampaignsModule(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder ConfigureHealthChecks(this WebApplicationBuilder builder)
    {
        string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
        string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;
        var rabbitMqSettings = new RabbitMqSettings(builder.Configuration.GetConnectionString("Queue")!);

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
            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/openapi/v1.json", "Lanka.Api"));
            app.ApplyMigrations();
        }

        app.MapEndpoints();
        app.MapHealthChecks("/healthz",
            new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
        );


        app.UseLogContext();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
