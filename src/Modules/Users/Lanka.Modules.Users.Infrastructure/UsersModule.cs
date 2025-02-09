using Lanka.Common.Infrastructure.Interceptors;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Infrastructure.Database;
using Lanka.Modules.Users.Infrastructure.Identity;
using Lanka.Modules.Users.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Users.Infrastructure
{
    public static class UsersModule
    {
        public static IServiceCollection AddUsersModule(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddInfrastructure(configuration);

            services.AddEndpoints(Presentation.AssemblyReference.Assembly);
            
            return services;
        }

        private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KeyCloakOptions>(configuration.GetSection("Users:KeyCloak"));

            services.AddTransient<KeyCloakAuthDelegatingHandler>();

            services
                .AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
                {
                    KeyCloakOptions keyCloakOptions = serviceProvider
                        .GetRequiredService<IOptions<KeyCloakOptions>>().Value;

                    httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
                })
                .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

            services.AddTransient<IIdentityProviderService, IdentityProviderService>();
            
            services.AddDbContext<UsersDbContext>((sp, options) =>
                options
                    .UseNpgsql(
                        configuration.GetConnectionString("Database"),
                        npgsqlOptions => npgsqlOptions
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
                    .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>())
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());
        }
    }
}
