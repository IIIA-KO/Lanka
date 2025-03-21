using Lanka.Common.Application.Caching;
using Lanka.Common.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lanka.Common.Application.Behaviors;

public class QueryCachingPipelineBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, ICachedQuery
    where TResponse : Result
{
    private static readonly Action<ILogger, string, Exception?> CacheHitLog =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1001, nameof(RequestLoggingPipelineBehavior<TRequest, TResponse>)),
            "Cache hit for {Query}"
        );
        
    private static readonly Action<ILogger, string, Exception?> CacheMissLog =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1001, nameof(RequestLoggingPipelineBehavior<TRequest, TResponse>)),
            "Processing request {RequestName}"
        );
        
    private readonly ICacheService _cacheService;
    private readonly ILogger<QueryCachingPipelineBehavior<TRequest, TResponse>> _logger;

    public QueryCachingPipelineBehavior(
        ICacheService cacheService, 
        ILogger<QueryCachingPipelineBehavior<TRequest, TResponse>> logger)
    {
        this._cacheService = cacheService;
        this._logger = logger;
    }


    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(request);
            
        TResponse? cachedResult = await this._cacheService.GetAsync<TResponse>(
            request.CacheKey,
            cancellationToken
        );

        string name = typeof(TRequest).Name;
            
        if (cachedResult is not null)
        {
            CacheHitLog(this._logger, name, null);
                
            return cachedResult;
        }
            
        CacheMissLog(this._logger, name, null);

        TResponse result = await next();

        if (result.IsSuccess)
        {
            await this._cacheService.SetAsync(
                request.CacheKey,
                result,
                request.Expiration,
                cancellationToken
            );
        }

        return result;
    }
}
