using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace MediQueue.API.Middleware;

/// <summary>
/// Protects the Hangfire Dashboard with Basic Authentication.
/// Credentials are loaded from IConfiguration (Hangfire:DashboardUsername / DashboardPassword).
/// In development the filter always allows access for convenience.
/// </summary>
public sealed class HangfireBasicAuthFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireBasicAuthFilter(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public bool Authorize([NotNull] DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Dev: allow unrestricted access for local debugging
        if (httpContext.RequestServices
                .GetService<IWebHostEnvironment>()
                ?.IsDevelopment() == true)
            return true;

        var header = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        try
        {
            var encoded = header["Basic ".Length..].Trim();
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var colonIdx = decoded.IndexOf(':');
            if (colonIdx < 0)
            {
                Challenge(httpContext);
                return false;
            }

            var user = decoded[..colonIdx];
            var pass = decoded[(colonIdx + 1)..];

            if (user == _username && pass == _password)
                return true;
        }
        catch
        {
            // fall through
        }

        Challenge(httpContext);
        return false;
    }

    private static void Challenge(HttpContext ctx)
    {
        ctx.Response.StatusCode = 401;
        ctx.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}
