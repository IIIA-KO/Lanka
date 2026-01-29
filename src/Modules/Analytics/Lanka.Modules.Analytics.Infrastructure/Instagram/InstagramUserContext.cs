using Lanka.Common.Application.Authentication;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

/// <summary>
/// Provides the current user's email for Instagram service resolution.
/// In HTTP context, reads from IUserContext. In background jobs, email must be set explicitly.
/// </summary>
internal sealed class InstagramUserContext : IInstagramUserContext
{
    private readonly IUserContext? _userContext;

    public InstagramUserContext(IUserContext? userContext = null)
    {
        this._userContext = userContext;
    }

    private string? ExplicitEmail { get; set; }

    public string? Email => this.ExplicitEmail ?? this.TryGetEmailFromHttpContext();

    public void SetEmail(string email)
    {
        this.ExplicitEmail = email;
    }

    private string? TryGetEmailFromHttpContext()
    {
        try
        {
            return this._userContext?.GetEmail();
        }
        catch
        {
            // No HTTP context available
            return null;
        }
    }
}
