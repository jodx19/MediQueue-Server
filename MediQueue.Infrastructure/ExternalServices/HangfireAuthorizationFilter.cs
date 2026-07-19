using Hangfire.Dashboard;
using System.Security.Claims;

namespace MediQueue.Infrastructure.ExternalServices;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext.User;

        // Restrict Hangfire Dashboard access to SuperAdmin role only
        return user.Identity?.IsAuthenticated == true && user.IsInRole("SuperAdmin");
    }
}
