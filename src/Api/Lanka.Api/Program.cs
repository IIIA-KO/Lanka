using Lanka.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .ConfigureBasicServices()
    .ConfigureLogging()
    .ConfigureModules()
    .ConfigureHealthChecks();

WebApplication app = builder.Build();

await app.ConfigureMiddleware();

await app.RunAsync();

#pragma warning disable CA1515 // Type can be made internal
namespace Lanka.Api
{
    public partial class Program;
}
