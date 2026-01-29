namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

/// <summary>
/// Provides ambient context for the current Instagram user's email.
/// Used to determine which Instagram service implementation to use in Development.
/// </summary>
public interface IInstagramUserContext
{
    /// <summary>
    /// Gets the current user's email, if available.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Sets the email for the current scope. Used by inbox/background processors
    /// before dispatching commands.
    /// </summary>
    void SetEmail(string email);
}
