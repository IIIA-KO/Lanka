using System.Threading.RateLimiting;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

public class InstagramApiDelegatingHandler : DelegatingHandler
{
    private readonly RateLimiter _rateLimiter;

    public InstagramApiDelegatingHandler(RateLimiter rateLimiter)
    {
        this._rateLimiter = rateLimiter;
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

        return await base.SendAsync(request, cancellationToken);
    }
}
