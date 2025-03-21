using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Lanka.Common.Presentation.Endpoints;

public abstract class EndpointBase : IEndpoint
{
    protected virtual string BaseRoute => string.Empty;
    protected virtual bool RequireAuthorization => true;
    protected virtual string? RateLimitingPolicy => null;
    protected virtual string[]? RequiredPermissions => null;
        
    protected string BuildRoute(string route) => 
        string.IsNullOrEmpty(route) ? this.BaseRoute : $"{this.BaseRoute}/{route}";
        
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteHandlerBuilder routeHandlerBuilder = this.MapEndpointInternal(app);

        if (this.RequireAuthorization)
        {
            if (this.RequiredPermissions is not null && this.RequiredPermissions.Length > 0)
            {
                routeHandlerBuilder.RequireAuthorization(this.RequiredPermissions);
            }
            else
            {
                routeHandlerBuilder.RequireAuthorization();
            }
        }

        if (!string.IsNullOrEmpty(this.RateLimitingPolicy))
        {
            routeHandlerBuilder.RequireRateLimiting(this.RateLimitingPolicy);
        }

        this.ConfigureEndpoint(routeHandlerBuilder);
    }

    protected abstract RouteHandlerBuilder MapEndpointInternal(IEndpointRouteBuilder app);
    
    protected virtual void ConfigureEndpoint(RouteHandlerBuilder builder)
    {
    }
}
