using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Lanka.Api.Middleware;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private sealed record InnerExceptionDetail(string Message, string? StackTrace);

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        this._logger = logger;
        this._environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        this._logger.LogError(
            exception,
            "Exception occurred: {Message}",
            exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Server failure",
            Detail = exception.Message,
        };

        if (this._environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;

            var innerExceptionDetails = new List<InnerExceptionDetail>();
            Exception? currentException = exception.InnerException;

            while (currentException != null)
            {
                innerExceptionDetails.Add(new InnerExceptionDetail(
                    currentException.Message,
                    currentException.StackTrace
                ));
                currentException = currentException.InnerException;
            }

            if (innerExceptionDetails.Any())
            {
                problemDetails.Extensions["innerExceptionDetails"] = innerExceptionDetails;
            }
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        string json = JsonSerializer.Serialize(problemDetails, _jsonSerializerOptions);

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }
}
