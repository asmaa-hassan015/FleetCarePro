using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using FleetCarePro.Data;
using FleetCarePro.Models;

namespace FleetCarePro.Filters
{
    public class AuditLogFilter : IActionFilter
    {
        private readonly FleetContext db;

        public AuditLogFilter(FleetContext context)
        {
            db = context;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Nothing needed before the action runs.
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                var userId = context.HttpContext.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                string controllerName =
                    context.RouteData.Values["controller"]?.ToString()
                    ?? string.Empty;

                string actionName =
                    context.RouteData.Values["action"]?.ToString()
                    ?? string.Empty;

                string details = context.Exception == null
                    ? "Succeeded"
                    : "Failed: " + context.Exception.Message;

                var log = new AuditLog
                {
                    UserId = userId,
                    ControllerName = controllerName,
                    ActionName = actionName,
                    Timestamp = DateTime.UtcNow,
                    Details = details
                };

                db.AuditLogs.Add(log);
                db.SaveChanges();
            }
            catch
            {
                // Audit logging failure should not break the main request.
            }
        }
    }
}