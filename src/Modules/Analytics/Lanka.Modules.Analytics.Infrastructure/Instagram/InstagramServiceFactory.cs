using Lanka.Common.Application.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

/// <summary>
/// Factory that resolves the appropriate Instagram service implementation at runtime.
/// Supports both HTTP request context (via IUserContext) and background job context (via explicit userId).
/// </summary>
internal sealed class InstagramServiceFactory<TService> : IInstagramServiceFactory<TService>
    where TService : class
{
    private readonly TService _realService;
    private readonly TService _mockService;
    private readonly IHostEnvironment _environment;
    private readonly InstagramDevelopmentOptions? _developmentOptions;
    private readonly IUserContext? _userContext;

    public InstagramServiceFactory(
        TService realService,
        TService mockService,
        IHostEnvironment environment,
        IOptions<InstagramDevelopmentOptions>? developmentOptions = null,
        IUserContext? userContext = null)
    {
        this._realService = realService;
        this._mockService = mockService;
        this._environment = environment;
        this._developmentOptions = developmentOptions?.Value;
        this._userContext = userContext;
    }

    public TService GetService(Guid? userId = null)
    {
        if (!this._environment.IsDevelopment())
        {
            return this._realService;
        }

        Guid? effectiveUserId = userId ?? this.TryGetUserIdFromContext();

        if (effectiveUserId.HasValue &&
            this._developmentOptions?.AllowedUserIds.Contains(effectiveUserId.Value) == true)
        {
            return this._realService;
        }

        return this._mockService;
    }

    private Guid? TryGetUserIdFromContext()
    {
        try
        {
            return this._userContext?.GetUserId();
        }
        catch
        {
            // No HTTP context available (e.g., in background jobs)
            return null;
        }
    }
}
