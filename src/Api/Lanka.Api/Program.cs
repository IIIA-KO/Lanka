WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Lanka.Api"));
}

app.UseHsts();
app.UseHttpsRedirection();

app.UseHttpsRedirection();

await app.RunAsync();
