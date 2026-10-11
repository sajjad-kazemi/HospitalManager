using PatientService.Application.Errors;
using System.Diagnostics;
using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RpcStatus = Grpc.Core.StatusCode;

namespace PatientService.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            return true;
        }

        var (status, title) = exception is ConflictException ? (409, "The change conflicts with an existing record.") : exception is RpcException rpc
            ? rpc.StatusCode switch
            {
                RpcStatus.InvalidArgument => (400, "The request is invalid."),
                RpcStatus.Unauthenticated => (401, "Authentication failed."),
                RpcStatus.PermissionDenied => (403, "You are not allowed to perform this operation."),
                RpcStatus.NotFound => (404, "The requested resource was not found."),
                RpcStatus.AlreadyExists => (409, "The resource already exists."),
                RpcStatus.Unavailable or RpcStatus.DeadlineExceeded => (503, "IdentityService is unavailable."),
                _ => (502, "IdentityService could not complete the request.")
            }
            : (500, "An unexpected error occurred.");

        if (exception is RpcException rpcException)
        {
            // Downstream RPC error details may contain sensitive data.
            if (status >= 500)
                logger.LogWarning("IdentityService call failed with RPC status {RpcStatus}.", rpcException.StatusCode);
            else
                logger.LogDebug("IdentityService rejected a request with RPC status {RpcStatus}.", rpcException.StatusCode);
        }
        else
            logger.LogError(exception, "An unexpected PatientService error occurred.");

        var problem = new ProblemDetails { Status = status, Title = title, Instance = httpContext.Request.Path };
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = status;

        if (!await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        }))
            await httpContext.Response.WriteAsJsonAsync(problem, options: (System.Text.Json.JsonSerializerOptions?)null,
                contentType: "application/problem+json", cancellationToken: cancellationToken);

        return true;
    }
}

