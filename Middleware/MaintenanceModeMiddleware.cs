using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FleetCarePro.Middleware
{
    public class MaintenanceModeMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;

        public MaintenanceModeMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            this.next = next;
            this.configuration = configuration;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            bool isMaintenanceMode =
                configuration.GetValue<bool>(
                    "IsMaintenanceMode");

            bool isStaticFile =
                context.Request.Path.StartsWithSegments("/css")
                || context.Request.Path.StartsWithSegments("/js")
                || context.Request.Path.StartsWithSegments("/lib")
                || context.Request.Path.StartsWithSegments("/uploads");

            bool isMaintenancePage =
                context.Request.Path.StartsWithSegments(
                    "/Home/Maintenance");

            if (isMaintenanceMode &&
                !isStaticFile &&
                !isMaintenancePage)
            {
                context.Response.Redirect(
                    "/Home/Maintenance");

                return;
            }

            await next(context);
        }
    }

    public static class MaintenanceModeMiddlewareExtensions
    {
        public static IApplicationBuilder UseMaintenanceMode(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<
                MaintenanceModeMiddleware>();
        }
    }
}