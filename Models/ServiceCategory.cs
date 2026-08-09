using System.ComponentModel.DataAnnotations;

namespace FleetCarePro.Models
{
    public class ServiceCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int RecommendedIntervalMonths { get; set; }

        public List<VendorService> VendorServices { get; set; } = new List<VendorService>();

        public List<ServiceLineItem> ServiceLineItems { get; set; } = new List<ServiceLineItem>();
    }
}