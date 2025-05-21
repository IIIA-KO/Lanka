using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Infrastructure.Outbox;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Photos;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Lanka.Modules.Campaigns.Domain.Reviews;
using Lanka.Modules.Campaigns.Infrastructure.Bloggers;
using Lanka.Modules.Campaigns.Infrastructure.Campaigns;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Lanka.Modules.Campaigns.Infrastructure.Inbox;
using Lanka.Modules.Campaigns.Infrastructure.Offers;
using Lanka.Modules.Campaigns.Infrastructure.Outbox;
using Lanka.Modules.Campaigns.Infrastructure.Pacts;
using Lanka.Modules.Campaigns.Infrastructure.Photos;
using Lanka.Modules.Campaigns.Infrastructure.Reviews;
using Lanka.Modules.Users.IntegrationEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lanka.Modules.Campaigns.Infrastructure;

public static class CampaignsModule
{
    public static IServiceCollection AddCampaignsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();

        services.AddIntegrationEventHandlers();

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        services.AddCloudinary(configuration);

        return services;
    }
    
    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
    {
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>()
            .Endpoint(configuration => configuration.InstanceId = instanceId);
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);

        AddOutbox(services, configuration);
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CampaignsDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Campaigns))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IBloggerRepository, BloggerRepository>();
        services.AddScoped<IPactRepository, PactRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CampaignsDbContext>());
    }

    private static void AddOutbox(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Campaigns:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        services.Configure<InboxOptions>(configuration.GetSection("Campaigns:Inbox"));
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
    
    private static void AddCloudinary(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CloudinaryOptions>(configuration.GetSection("Cloudinary"));

        services.AddSingleton<IPhotoAccessor, PhotoAccessor>();
    }
}
