using System.Reflection;
using FluentValidation;
using Lanka.Common.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.Common.Application
{
    public static class ApplicationConfiguration
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            Assembly[] assemblies
        )
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(assemblies);

                config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
                config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
                config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
                config.AddOpenBehavior(typeof(QueryCachingPipelineBehavior<,>));
            });

            services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

            return services;
        }
    }
}
