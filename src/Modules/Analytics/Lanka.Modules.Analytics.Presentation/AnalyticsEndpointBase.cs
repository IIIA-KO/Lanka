using Lanka.Common.Presentation.Endpoints;
using Lanka.Modules.Analytics.Presentation.RateLimiting;

namespace Lanka.Modules.Analytics.Presentation;

public abstract class AnalyticsEndpointBase : EndpointBase
{
    protected override string BaseRoute => "analytics";

    protected override string RateLimitingPolicy => RateLimitingConfig.InstagramPolicy;
}
