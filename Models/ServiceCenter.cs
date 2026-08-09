using System.ComponentModel.DataAnnotations;

namespace FleetCarePro.Models
{
    public class ServiceCenter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; }

        public List<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();

        public List<VendorService> VendorServices { get; set; } = new List<VendorService>();
    }
}