using Lanka.Api.Middleware;
using Lanka.Common.Application;
using Lanka.Common.Infrastructure;
using Lanka.Common.Infrastructure.EventBus;
using Lanka.Common.Infrastructure.Notifications;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Analytics.Infrastructure;
using Lanka.Modules.Campaigns.Infrastructure;
using Lanka.Modules.Matching.Infrastructure;
using Lanka.Modules.Users.Infrastructure;
using Lanka.ServiceDefaults;
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
        builder.Host.UseSerilog(
            (context, loggerConfiguration) =>
                loggerConfiguration.ReadFrom.Configuration(context.Configuration),
            writeToProviders: true);

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
            builder.Environment.ApplicationName,
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
        builder.Services.AddAnalyticsModule(builder.Configuration, builder.Environment);
        builder.Services.AddMatchingModule(builder.Configuration);

        builder.Services.AddSignalR();

        return builder;
    }

    public static async Task ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.ApplyMigrations();
            await app.SeedDevelopmentDataAsync();
        }

        app.UseLogContextTraceLogging();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
        app.MapDefaultEndpoints();

        app.MapHub<InstagramHub>("/hubs/instagram");
    }
}
