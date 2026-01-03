using System.ComponentModel.DataAnnotations;
using System.Net;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    readonly RequestDelegate next;
    readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger
    )
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            var response = BuildErrorResponse(ex);
            httpContext.Response.StatusCode = response.StatusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }

    ErrorResponse BuildErrorResponse(Exception ex)
    {
        var statusCode = ex switch
        {
            // 400 Bad Request
            ArgumentNullException or ArgumentOutOfRangeException or ArgumentException 
                or ValidationException or InvalidOperationException
                => HttpStatusCode.BadRequest,

            // 401 Unauthorized
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,

            // 404 Not Found
            KeyNotFoundException => HttpStatusCode.NotFound,

            // 408 Request Timeout
            TimeoutException => HttpStatusCode.RequestTimeout,

            // TODO: メンテナンス等によるサービスの停止中 503 ServiceUnavailable
            
            // 500 Internal Server Error
            // NariNoteException はカスタムのエラーであるため可読性の為 500 であることを明記
            NariNoteException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.InternalServerError,
        };

        return new ErrorResponse()
        {
            StatusCode = statusCode.AsInt(),
            Message = ex.Message,
        };
    }
}
