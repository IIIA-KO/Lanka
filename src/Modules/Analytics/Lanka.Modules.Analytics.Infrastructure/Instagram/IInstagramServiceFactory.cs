namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

/// <summary>
/// Factory for resolving Instagram service implementations based on user context.
/// In Development environment, this allows per-user resolution of real vs mock services
/// based on the configured AllowedUserIds list.
/// </summary>
/// <typeparam name="TService">The Instagram service interface type.</typeparam>
public interface IInstagramServiceFactory<TService> where TService : class
{
    /// <summary>
    /// Gets the appropriate service implementation for the given user.
    /// </summary>
    /// <param name="userId">
    /// The user ID to determine service selection.
    /// In Development: returns real service if userId is in AllowedUserIds, mock otherwise.
    /// In Production: always returns real service.
    /// If null and in HTTP context, attempts to resolve from IUserContext.
    /// </param>
    /// <returns>The appropriate service implementation.</returns>
    TService GetService(Guid? userId = null);
}
