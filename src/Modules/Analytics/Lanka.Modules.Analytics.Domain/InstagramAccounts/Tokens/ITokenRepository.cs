namespace Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;

public interface ITokenRepository
{
    public void Add(Token token);
    
    public void Remove(Token token);
}
