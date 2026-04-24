using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Middleware;

public class TransactionMiddleware
{
    readonly RequestDelegate next;

    public TransactionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, NariNoteDbContext dbContext)
    {
        if (HttpMethods.IsGet(httpContext.Request.Method) || HttpMethods.IsHead(httpContext.Request.Method))
        {
            await next(httpContext);
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await next(httpContext);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            // Error は別 Middleware で catch し一元管理するので、そのまま throw する
            throw;
        }
    }
}
