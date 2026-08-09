using Microsoft.AspNetCore.Mvc;

namespace FleetCarePro.Filters
{
    public class AuditLogAttribute : TypeFilterAttribute
    {
        public AuditLogAttribute()
            : base(typeof(AuditLogFilter))
        {
        }
    }
}