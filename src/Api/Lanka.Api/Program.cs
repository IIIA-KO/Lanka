WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.RunAsync();
