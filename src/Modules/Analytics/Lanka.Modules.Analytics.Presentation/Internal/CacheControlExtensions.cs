using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Lanka.Modules.Analytics.Presentation.Internal;

internal static class CacheControlExtensions
{
    /// <summary>
    /// Sets a private Cache-Control max-age on the endpoint's response so the browser can
    /// reuse the body for the configured number of seconds. Use only on read-only GETs
    /// whose payloads are safe to serve stale within that window.
    /// </summary>
    public static RouteHandlerBuilder WithBrowserCache(this RouteHandlerBuilder builder, int seconds)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            object? result = await next(context);

            HttpResponse response = context.HttpContext.Response;
            if (!response.HasStarted)
            {
                response.Headers.CacheControl = $"private, max-age={seconds}";
            }

            return result;
        });
    }
}
