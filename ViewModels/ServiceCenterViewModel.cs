using System.ComponentModel.DataAnnotations;

namespace FleetCarePro.ViewModels
{
    public class ServiceCenterViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; }

        // Selected ServiceCategory Ids
        // for the N-to-N VendorServices relationship
        public List<int> SelectedServiceCategoryIds { get; set; }
            = new List<int>();

        // Dropdown data source
        public List<Models.ServiceCategory> ServiceCategories { get; set; }
            = new List<Models.ServiceCategory>();
    }
}