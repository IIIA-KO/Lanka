namespace Lanka.Common.Application.Authentication
{
    public interface IUserContext
    {
        Guid GetUserId();

        string GetIdentityId();
        
        string? AccessToken { get; }
    }
}
