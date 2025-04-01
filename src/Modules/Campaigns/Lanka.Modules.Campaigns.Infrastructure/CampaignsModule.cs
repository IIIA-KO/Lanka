using Lanka.Common.Infrastructure.Interceptors;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using Lanka.Modules.Campaigns.Infrastructure.Bloggers;
using Lanka.Modules.Campaigns.Infrastructure.Campaigns;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Lanka.Modules.Campaigns.Infrastructure.Offers;
using Lanka.Modules.Campaigns.Infrastructure.Pacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.Modules.Campaigns.Infrastructure;

public static class CampaignsModule
{
    public static IServiceCollection AddCampaignsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);
            
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CampaignsDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Campaigns))
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>())
                .UseSnakeCaseNamingConvention());
        
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IBloggerRepository, BloggerRepository>();
        services.AddScoped<IPactRepository, PactRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CampaignsDbContext>());
    }
}
