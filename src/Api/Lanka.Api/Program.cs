using Lanka.Api.Extensions;
using Lanka.Api.Middleware;
using Lanka.Common.Application;
using Lanka.Common.Infrastructure;
using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Users.Infrastructure;
using HealthChecks.UI.Client;
using Lanka.Modules.Campaigns.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Host.UseSerilog(
    (context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication([
    Lanka.Modules.Users.Application.AssemblyReference.Assembly
]);

string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;

builder.Services.AddInfrastructure(
    [],
    databaseConnectionString, 
    redisConnectionString
);

builder.Configuration.AddModuleConfiguration(["users","campaigns"]);

builder
    .Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString)
    .AddUrlGroup(new Uri(builder.Configuration.GetValue<string>("KeyCloak:HealthUrl")!), HttpMethod.Get, "keycloack");

builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddCampaignsModule(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Lanka.Api"));
    
    app.ApplyMigrations();
}

app.MapEndpoints();

app.MapHealthChecks(
    "/healthz",
    new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
);

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

await app.RunAsync();
