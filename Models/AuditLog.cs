using System.ComponentModel.DataAnnotations;

namespace FleetCarePro.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string ActionName { get; set; } = string.Empty;

        public string ControllerName { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? Details { get; set; }
    }
}