namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

/// <summary>
/// Factory for resolving Instagram service implementations based on user context.
/// In Development environment, this allows per-user resolution of real vs mock services
/// based on the configured AllowedUserEmails list.
/// </summary>
/// <typeparam name="TService">The Instagram service interface type.</typeparam>
public interface IInstagramServiceFactory<out TService> where TService : class
{
    /// <summary>
    /// Gets the appropriate service implementation for the given user.
    /// </summary>
    /// <param name="email">
    /// The user email to determine service selection.
    /// In Development: returns real service if email is in AllowedUserEmails, mock otherwise.
    /// In Production: always returns real service.
    /// If null and in HTTP context, attempts to resolve from IUserContext.
    /// </param>
    /// <returns>The appropriate service implementation.</returns>
    TService GetService(string? email = null);
}
