using Polly;
using Yarp.ReverseProxy.Forwarder;

namespace Lanka.Gateway.Resiliency;

internal sealed class ResilientHttpClientFactory : ForwarderHttpClientFactory
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public ResilientHttpClientFactory(ResiliencePipeline<HttpResponseMessage> pipeline)
    {
        this._pipeline = pipeline;
    }

    protected override HttpMessageHandler WrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
    {
        return new ResilienceDelegatingHandler(handler, this._pipeline);
    }
}
