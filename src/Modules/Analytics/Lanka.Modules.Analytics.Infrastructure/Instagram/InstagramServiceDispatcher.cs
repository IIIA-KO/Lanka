using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

/// <summary>
/// Dispatcher that routes Instagram service calls based on environment and target user.
/// - Production/Staging: Always uses real API
/// - Development + Target user in allowed list: Uses real API
/// - Development + Target user NOT in allowed list: Uses mock services
/// </summary>
internal sealed class InstagramServiceDispatcher<TService> where TService : class
{
    private readonly TService _realService;
    private readonly TService _mockService;
    private readonly IHostEnvironment _environment;
    private readonly InstagramDevelopmentOptions? _developmentOptions;

    public InstagramServiceDispatcher(
        TService realService,
        TService mockService,
        IHostEnvironment environment,
        IOptions<InstagramDevelopmentOptions>? developmentOptions = null)
    {
        this._realService = realService;
        this._mockService = mockService;
        this._environment = environment;
        this._developmentOptions = developmentOptions?.Value;
    }

    /// <summary>
    /// Gets the appropriate service based on environment and target user.
    /// In Development, returns real service if userId is in allowed list, otherwise mock.
    /// In Production/Staging, always returns real service.
    /// </summary>
    /// <param name="userId">Optional user ID to check against allowed list (only used in Development)</param>
    public TService GetService(Guid? userId = null)
    {
        // Production/Staging always uses real API
        if (!this._environment.IsDevelopment())
        {
            return this._realService;
        }

        // Development: check if target user is in allowed list
        if (userId.HasValue &&
            this._developmentOptions?.AllowedUserIds.Contains(userId.Value) == true)
        {
            return this._realService;
        }

        // Default to mock service in development
        return this._mockService;
    }

    /// <summary>
    /// Gets the appropriate service based on InstagramAccount's user.
    /// </summary>
    public TService GetService(InstagramAccount account)
    {
        return this.GetService(account.UserId.Value);
    }
}
