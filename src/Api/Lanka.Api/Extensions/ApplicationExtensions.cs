using Lanka.Api.Middleware;
using Lanka.Common.Application;
using Lanka.Common.Infrastructure;
using Lanka.Common.Infrastructure.Notifications;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Analytics.Infrastructure;
using Lanka.Modules.Campaigns.Infrastructure;
using Lanka.Modules.Matching.Infrastructure;
using Lanka.Modules.Users.Infrastructure;
using Lanka.ServiceDefaults;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Scalar.AspNetCore;
using Serilog;

namespace Lanka.Api.Extensions;

internal static class ApplicationExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder ConfigureBasicServices()
        {
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddOpenApi();

            return builder;
        }

        public WebApplicationBuilder ConfigureLogging()
        {
            builder.Host.UseSerilog(
                configureLogger: (context, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration),
                writeToProviders: true
            );

            return builder;
        }

        public WebApplicationBuilder ConfigureModules()
        {
            builder.Services.AddApplication([
                Modules.Users.Application.AssemblyReference.Assembly,
                Modules.Campaigns.Application.AssemblyReference.Assembly,
                Modules.Analytics.Application.AssemblyReference.Assembly,
                Modules.Matching.Application.AssemblyReference.Assembly
            ]);

            builder.AddRedisClient("Cache");

            builder.AddMongoDBClient("Mongo");
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            builder.Services.AddInfrastructure(
                builder.Configuration,
                builder.Environment.ApplicationName,
                moduleConfigureConsumers:
                [
                    UsersModule.ConfigureConsumers(builder.Configuration),
                    CampaignsModule.ConfigureConsumers,
                    AnalyticsModule.ConfigureConsumers,
                    MatchingModule.ConfigureConsumers
                ]
            );

            builder.Configuration.AddModuleConfiguration(["users", "campaigns", "analytics", "matching"]);
            builder.Services.AddUsersModule(builder.Configuration);
            builder.Services.AddCampaignsModule(builder.Configuration);
            builder.Services.AddAnalyticsModule(builder.Configuration, builder.Environment);
            builder.Services.AddMatchingModule(builder.Configuration);

            builder.Services.AddSignalR();

            return builder;
        }
    }

    extension(WebApplication app)
    {
        public async Task ConfigureMiddleware()
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
}
