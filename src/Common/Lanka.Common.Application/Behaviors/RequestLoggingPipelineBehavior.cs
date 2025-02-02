using Lanka.Common.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Lanka.Common.Application.Behaviors
{
    internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class
        where TResponse : Result
    {
        private static readonly Action<ILogger, string, Exception?> ProcessingRequestLog =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1001, nameof(RequestLoggingPipelineBehavior<TRequest, TResponse>)),
                "Processing request {RequestName}"
            );

        private static readonly Action<ILogger, string, Exception?> CompletedRequestLog =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1002, nameof(RequestLoggingPipelineBehavior<TRequest, TResponse>)),
                "Completed request {RequestName}"
            );

        private static readonly Action<ILogger, string, string, Exception?> ErrorRequestLog =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(1003, nameof(RequestLoggingPipelineBehavior<TRequest, TResponse>)),
                "Completed request {RequestName} with error: {Error}"
            );
        
        private readonly ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public RequestLoggingPipelineBehavior(
            ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger
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
            string moduleName = GetModuleName(typeof(TRequest).FullName!);
            string requestName = typeof(TRequest).Name;

            using (LogContext.PushProperty("Module", moduleName))
            {
                ProcessingRequestLog(this._logger, requestName, null);

                TResponse result = await next();

                if (result.IsSuccess)
                {
                    CompletedRequestLog(this._logger, requestName, null);
                }
                else
                {
                    using (LogContext.PushProperty("Error", result.Error, true))
                    {
                        ErrorRequestLog(this._logger, requestName, result.Error.Description, null);
                    }
                }

                return result;
            }
        }

        private static string GetModuleName(string requestName)
        {
            return requestName.Split('.')[2];
        }
    }
}
