using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;

namespace NariNoteBackend.Infrastructure.Middleware;

/// <summary>
/// グローバル例外ハンドラーミドルウェア
/// すべての例外を一括でキャッチし、統一されたエラーレスポンスを返却
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (NariNoteBackend.Application.Exception.ApplicationException ex)
        {
            await this.HandleApplicationExceptionAsync(context, ex);
        }
        catch (System.Exception ex)
        {
            await this.HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// アプリケーション例外を処理
    /// </summary>
    private async Task HandleApplicationExceptionAsync(
        HttpContext context,
        NariNoteBackend.Application.Exception.ApplicationException exception)
    {
        this.logger.LogWarning(
            exception,
            "Application exception occurred: {ErrorCode} - {Message}",
            exception.ErrorCode,
            exception.Message);

        var response = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = exception.ErrorCode,
                Message = exception.Message,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                AdditionalData = exception.AdditionalData
            }
        };

        context.Response.StatusCode = exception.StatusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// 予期しない例外を処理
    /// </summary>
    private async Task HandleUnexpectedExceptionAsync(
        HttpContext context,
        System.Exception exception)
    {
        this.logger.LogError(
            exception,
            "Unexpected exception occurred: {Message}",
            exception.Message);

        var response = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = "INTERNAL_SERVER_ERROR",
                Message = "An unexpected error occurred",
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path
            }
        };

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
