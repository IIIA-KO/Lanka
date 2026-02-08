using Lanka.Gateway.Extensions;
using Lanka.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder
    .ConfigureCors()
    .ConfigureLogging()
    .ConfigureRateLimiting()
    .ConfigureReverserProxy()
    .ConfigureAuthentication();

WebApplication app = builder.Build();

app.ConfigureMiddleware();

await app.RunAsync();
