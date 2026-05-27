using Polly;

namespace Lanka.Gateway.Resiliency;

internal sealed class ResilienceDelegatingHandler : DelegatingHandler
{
    private static readonly PathString _hubPathPrefix = new("/hubs");

    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public ResilienceDelegatingHandler(
        HttpMessageHandler innerHandler,
        ResiliencePipeline<HttpResponseMessage> pipeline
    )
    {
        this.InnerHandler = innerHandler;
        this._pipeline = pipeline;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (IsLongLivedRequest(request))
        {
            return base.SendAsync(request, cancellationToken);
        }

        return this._pipeline.ExecuteAsync(
            async ct => await base.SendAsync(request, ct).ConfigureAwait(false),
            cancellationToken
        ).AsTask();
    }

    private static bool IsLongLivedRequest(HttpRequestMessage request)
    {
        bool isWebSocket = request.Headers.Upgrade.Any(value =>
            string.Equals(value.Name, "websocket", StringComparison.OrdinalIgnoreCase));

        bool isHubRequest = request.RequestUri is not null
                            && new PathString(request.RequestUri.AbsolutePath).StartsWithSegments(_hubPathPrefix);

        bool isServerSentEvent = request.Headers.Accept.Any(value =>
            string.Equals(value.MediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase));

        return isWebSocket || isHubRequest || isServerSentEvent;
    }
}
