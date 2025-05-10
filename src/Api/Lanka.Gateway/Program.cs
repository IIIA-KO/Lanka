using Lanka.Gateway.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .ConfigureLogging()
    .ConfigureRateLimiting()
    .ConfigureReverserProxy()
    .ConfigureTracing()
    .ConfigureAuthentication();

WebApplication app = builder.Build();

app.ConfigureMiddleware();

await app.RunAsync();
