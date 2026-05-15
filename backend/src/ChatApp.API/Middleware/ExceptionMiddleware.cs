using System.Net;
using System.Text.Json;

namespace ChatApp.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "İşlenmeyen hata: {Message}", ex.Message);
            await HandleAsync(ctx, ex);
        }
    }

    private static Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,  ex.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound,       ex.Message),
            InvalidOperationException   => (HttpStatusCode.BadRequest,     ex.Message),
            ArgumentException           => (HttpStatusCode.BadRequest,     ex.Message),
            _                           => (HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu.")
        };

        ctx.Response.StatusCode  = (int)status;
        ctx.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { error = message });
        return ctx.Response.WriteAsync(body);
    }
}