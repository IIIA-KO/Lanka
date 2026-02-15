using System.Threading.RateLimiting;
using Lanka.Gateway.Middleware;
using Lanka.Gateway.RateLimiting;
using Lanka.Gateway.Resiliency;
using Lanka.ServiceDefaults;
using Polly;
using Serilog;
using Yarp.ReverseProxy.Forwarder;
using CorsOptions = Lanka.Gateway.Cors.CorsOptions;

namespace Lanka.Gateway.Extensions;

internal static class ApplicationExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder ConfigureCors()
        {
            CorsOptions corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()!;

            builder.Services.AddCors(options =>
                options.AddPolicy(CorsOptions.PolicyName, corsPolicyBuilder =>
                    corsPolicyBuilder
                        .WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                )
            );

            return builder;
        }

        public WebApplicationBuilder ConfigureLogging()
        {
            builder.Host.UseSerilog(
                (context, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration),
                writeToProviders: true);

            return builder;
        }

        public WebApplicationBuilder ConfigureRateLimiting()
        {
            builder
                .Services
                .AddRateLimiter(options =>
                    {
                        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                        options.AddPolicy(RateLimitingConfig.FixedByIp, httpContext =>
                            RateLimitPartition.GetFixedWindowLimiter(
                                partitionKey: httpContext.Request.Headers["X-Forwarded-For"].ToString()
                                              ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                              ?? "unknown",
                                factory: _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 100,
                                    Window = TimeSpan.FromMinutes(1),
                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                    QueueLimit = 0
                                }
                            )
                        );
                    }
                );

            builder
                .Services
                .AddRateLimiter(options =>
                    {
                        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                        options.AddPolicy(RateLimitingConfig.InstagramPolicy, httpContext =>
                            RateLimitPartition.GetFixedWindowLimiter(
                                partitionKey: httpContext.Request.Headers["X-Forwarded-For"].ToString()
                                              ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                              ?? "unknown",
                                factory: _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 10,
                                    Window = TimeSpan.FromMinutes(1),
                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                    QueueLimit = 0
                                }
                            )
                        );
                    }
                );

            return builder;
        }

        public WebApplicationBuilder ConfigureReverserProxy()
        {
            ResiliencePipeline<HttpResponseMessage> pipeline = ResiliencePolicyBuilder.Build();

            builder.Services.AddSingleton<IForwarderHttpClientFactory>(_ => new ResilientHttpClientFactory(pipeline)
            );

            builder
                .Services
                .AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                .AddServiceDiscoveryDestinationResolver();

            return builder;
        }

        public WebApplicationBuilder ConfigureAuthentication()
        {
            builder.Services.AddAuthorization();

            builder.Services.AddAuthentication().AddJwtBearer();

            return builder;
        }
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        app.UseCors(CorsOptions.PolicyName);

        app.UseLogContextTraceLogging();
        app.UseSerilogRequestLogging();

        app.UseWebSockets();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapReverseProxy()
            .RequireRateLimiting(RateLimitingConfig.FixedByIp);

        app.MapDefaultEndpoints();

        return app;
    }
}
