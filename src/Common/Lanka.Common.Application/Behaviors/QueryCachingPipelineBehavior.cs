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
    private static readonly Action<ILogger, string, Exception?> CacheMissLog =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1001, nameof(RequestLoggingPipelineBehavior<,>)),
            "Processing request {RequestName}"
        );

    private readonly ICacheService _cacheService;
    private readonly ILogger<QueryCachingPipelineBehavior<TRequest, TResponse>> _logger;

    public QueryCachingPipelineBehavior(
        ICacheService cacheService,
        ILogger<QueryCachingPipelineBehavior<TRequest, TResponse>> logger
    )
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

        string name = typeof(TRequest).Name;

        return await this._cacheService.GetOrCreateAsync(
            request.CacheKey,
            factory: async _ =>
            {
                CacheMissLog(this._logger, name, null);
                return await next(cancellationToken);
            },
            request.Expiration,
            cancellationToken
        );
    }
}
