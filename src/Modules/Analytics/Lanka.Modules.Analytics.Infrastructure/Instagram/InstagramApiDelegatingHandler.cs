using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

public partial class InstagramApiDelegatingHandler : DelegatingHandler
{
    private readonly RateLimiter _rateLimiter;
    private readonly ILogger<InstagramApiDelegatingHandler> _logger;

    public InstagramApiDelegatingHandler(
        RateLimiter rateLimiter,
        ILogger<InstagramApiDelegatingHandler> logger)
    {
        this._rateLimiter = rateLimiter;
        this._logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        using RateLimitLease lease = await this._rateLimiter
            .AcquireAsync(permitCount: 1, cancellationToken);

        if (!lease.IsAcquired)
        {
            throw new HttpRequestException("Instagram API rate limit exceeded. Try again later.");
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string body = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken);

            string url = Sanitize(request.RequestUri?.ToString() ?? string.Empty);

            this._logger.LogError(
                "Instagram API {Method} {Url} failed: {Status} body={Body}",
                request.Method,
                url,
                (int)response.StatusCode,
                body);
        }

        return response;
    }

    private static string Sanitize(string url) =>
        AccessTokenRegex().Replace(url, "access_token=***");

    [GeneratedRegex("access_token=[^&]*")]
    private static partial Regex AccessTokenRegex();
}
