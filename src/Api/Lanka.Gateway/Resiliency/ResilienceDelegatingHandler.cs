using Polly;

namespace Lanka.Gateway.Resiliency;

internal sealed class ResilienceDelegatingHandler : DelegatingHandler
{
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
        return this._pipeline.ExecuteAsync(
            async ct => await base.SendAsync(request, ct).ConfigureAwait(false),
            cancellationToken
        ).AsTask();
    }
}
