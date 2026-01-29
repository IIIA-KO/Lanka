using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

/// <summary>
/// Factory that resolves the appropriate Instagram service implementation at runtime.
/// Uses IInstagramUserContext to get the current user's email for service resolution.
/// </summary>
internal sealed class InstagramServiceFactory<TService> : IInstagramServiceFactory<TService>
    where TService : class
{
    private readonly TService _realService;
    private readonly TService _mockService;
    private readonly IHostEnvironment _environment;
    private readonly InstagramDevelopmentOptions? _developmentOptions;
    private readonly IInstagramUserContext? _instagramUserContext;

    public InstagramServiceFactory(
        TService realService,
        TService mockService,
        IHostEnvironment environment,
        IOptions<InstagramDevelopmentOptions>? developmentOptions = null,
        IInstagramUserContext? instagramUserContext = null)
    {
        this._realService = realService;
        this._mockService = mockService;
        this._environment = environment;
        this._developmentOptions = developmentOptions?.Value;
        this._instagramUserContext = instagramUserContext;
    }

    public TService GetService(string? email = null)
    {
        if (!this._environment.IsDevelopment())
        {
            return this._realService;
        }

        string? effectiveEmail = email ?? this._instagramUserContext?.Email;

        if (effectiveEmail is not null &&
            this._developmentOptions?.AllowedUserEmails.Contains(effectiveEmail, StringComparer.OrdinalIgnoreCase) == true)
        {
            return this._realService;
        }

        return this._mockService;
    }
}
