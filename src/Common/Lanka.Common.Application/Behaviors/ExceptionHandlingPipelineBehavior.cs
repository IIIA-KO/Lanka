using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lanka.Common.Application.Behaviors;

internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    private static readonly Action<ILogger, string, Exception?> ErrorLog =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1003, nameof(RequestLoggingPipelineBehavior<TRequest, TResponse>)),
            "Unhandled exception for {RequestName}"
        );
        
    private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingPipelineBehavior(
        ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger
    )
    {
        this._logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception)
        {
            ErrorLog(this._logger, typeof(TRequest).Name, null);

            throw new LankaException(typeof(TRequest).Name, exception);
        }
    }
}
