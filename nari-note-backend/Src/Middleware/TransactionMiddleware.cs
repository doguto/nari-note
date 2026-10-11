using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NariNoteBackend.Filter;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Middleware;

public class TransactionMiddleware
{
    static readonly TimeSpan SlowTransactionThreshold = TimeSpan.FromSeconds(1);

    readonly ILogger<TransactionMiddleware> logger;
    readonly RequestDelegate next;

    public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, NariNoteDbContext dbContext)
    {
        if (ShouldSkipTransaction(httpContext))
        {
            await next(httpContext);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var outcome = "Rollback";

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await next(httpContext);
            await transaction.CommitAsync();
            outcome = "Commit";
        }
        catch
        {
            await transaction.RollbackAsync();

            // Error は別 Middleware で catch し一元管理するので、そのまま throw する
            throw;
        }
        finally
        {
            LogTransactionDuration(httpContext, stopwatch.Elapsed, outcome);
        }
    }

    static bool ShouldSkipTransaction(HttpContext httpContext)
    {
        if (HttpMethods.IsGet(httpContext.Request.Method) || HttpMethods.IsHead(httpContext.Request.Method))
        {
            return true;
        }

        return httpContext.GetEndpoint()?.Metadata.GetMetadata<NoTransactionAttribute>() != null;
    }

    void LogTransactionDuration(HttpContext httpContext, TimeSpan elapsed, string outcome)
    {
        var level = elapsed >= SlowTransactionThreshold ? LogLevel.Warning : LogLevel.Information;
        logger.Log(
            level,
            "Transaction finished. Outcome={Outcome} ElapsedMs={ElapsedMs} Method={Method} Path={Path}",
            outcome,
            (long)elapsed.TotalMilliseconds,
            httpContext.Request.Method,
            httpContext.Request.Path.Value
        );
    }
}
