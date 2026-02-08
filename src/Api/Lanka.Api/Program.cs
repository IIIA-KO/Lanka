using Lanka.Api.Extensions;
using Lanka.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder
    .ConfigureBasicServices()
    .ConfigureLogging()
    .ConfigureModules();

WebApplication app = builder.Build();

await app.ConfigureMiddleware();

await app.RunAsync();

#pragma warning disable CA1515 // Type can be made internal
namespace Lanka.Api
{
    public partial class Program;
}
