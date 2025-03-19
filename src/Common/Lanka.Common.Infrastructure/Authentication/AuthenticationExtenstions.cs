using Lanka.Common.Application.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.Common.Infrastructure.Authentication
{
    internal static class AuthenticationExtenstions
    {
        internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
        {
            services.AddAuthorization();
            
            services.AddAuthentication().AddJwtBearer();
            
            services.AddHttpContextAccessor();

            services.ConfigureOptions<JwtBearerConfigureOptions>();
            
            services.AddHttpContextAccessor();

            services.AddScoped<IUserContext, UserContext>();
            
            return services;
        }
    }
}
