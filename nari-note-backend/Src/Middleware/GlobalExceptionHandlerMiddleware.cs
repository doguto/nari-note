using System.Net;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Dto.Response;
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
        return ex switch
        {

            ArgumentNullException or ArgumentOutOfRangeException or ArgumentException
                => new ErrorResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest.AsInt(),
                    Message = ex.Message,
                },
            KeyNotFoundException 
                => new ErrorResponse()
                {
                    StatusCode = HttpStatusCode.NotFound.AsInt(),
                    Message = ex.Message,
                },
            UnauthorizedAccessException
                => new ErrorResponse()
                {
                    StatusCode = HttpStatusCode.Unauthorized.AsInt(),
                    Message = ex.Message,
                },
            InvalidOperationException
                => new ErrorResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest.AsInt(),
                    Message = ex.Message,
                },
            DbUpdateConcurrencyException
                => new ErrorResponse()
                {
                    StatusCode = HttpStatusCode.Conflict.AsInt(),
                    Message = ex.Message,
                },
            DbUpdateException
                => new ErrorResponse()
                {
                    StatusCode = HttpStatusCode.InternalServerError.AsInt(),
                    Message = ex.Message,
                },
            TimeoutException
                => new ErrorResponse()
                {
                    StatusCode = HttpStatusCode.RequestTimeout.AsInt(),
                    Message = ex.Message,
                },
            _ => new ErrorResponse()
            {
                StatusCode = HttpStatusCode.InternalServerError.AsInt(),
                Message = ex.Message,
            }
        };
    }
}
