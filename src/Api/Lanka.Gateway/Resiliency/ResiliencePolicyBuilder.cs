using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;

namespace Lanka.Gateway.Resiliency;

internal static class ResiliencePolicyBuilder
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    public static ResiliencePipeline<HttpResponseMessage> Build()
    {
        PredicateBuilder<HttpResponseMessage> errorPredicate = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(response => (int)response.StatusCode >= 500);

        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(TimeSpan.FromMinutes(1))
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(1),
                ShouldHandle = errorPredicate
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = errorPredicate,
                FailureRatio = 0.5,
                MinimumThroughput = 20,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15)
            })
            .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = errorPredicate,
                FallbackAction = _ =>
                {
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status503ServiceUnavailable,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.4",
                        Title = "Service failure",
                        Detail = "The service is currently unavailable. Please try again later.",
                    };

                    string json = JsonSerializer.Serialize(problemDetails, _jsonSerializerOptions);
                    
                    var content = new StringContent(json);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/problem+json");

                    var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = content
                    };
                    
                    return new ValueTask<Outcome<HttpResponseMessage>>(Outcome.FromResult(response));
                }
            })
            .Build();
    }
}
