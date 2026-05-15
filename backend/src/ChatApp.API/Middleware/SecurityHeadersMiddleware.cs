namespace ChatApp.API.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        // Clickjacking 
        ctx.Response.Headers["X-Frame-Options"] = "DENY";

        // XSS 
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Referrer 
        ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // HTTPS required (production)
        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // asllowed resources
        ctx.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "connect-src 'self' ws://localhost:5000 wss://localhost:5000; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline';";

        await next(ctx);
    }
}