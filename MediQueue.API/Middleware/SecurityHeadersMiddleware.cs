namespace MediQueue.API.Middleware;

/// <summary>
/// Middleware to add OWASP-recommended security response headers to every HTTP response.
/// Addresses: X-Content-Type-Options, X-Frame-Options, Strict-Transport-Security,
/// Content-Security-Policy, Referrer-Policy, Permissions-Policy.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent MIME-type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // Force HTTPS for 1 year (only in non-dev environments, but middleware
        // is only registered in non-dev — see Program.cs)
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

        // Restrict resource loading — tightened for an API (no scripts/styles served)
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'none'; " +
            "frame-ancestors 'none'; " +
            "form-action 'none'";

        // Don't send Referer header when navigating away
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Disable browser features that are not needed for an API
        context.Response.Headers["Permissions-Policy"] =
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
            "magnetometer=(), microphone=(), payment=(), usb=()";

        // Remove server identity header added by Kestrel / ASP.NET
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        await _next(context);
    }
}

/// <summary>Extension methods to register SecurityHeadersMiddleware.</summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
