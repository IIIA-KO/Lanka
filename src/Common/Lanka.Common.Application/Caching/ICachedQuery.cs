namespace Lanka.Common.Application.Caching
{
    public interface ICachedQuery<TReposnse> : Messaging.IQuery<TReposnse>, ICachedQuery;
    
    public interface ICachedQuery
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
    }
}
