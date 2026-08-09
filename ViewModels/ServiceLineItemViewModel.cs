using System.ComponentModel.DataAnnotations;

namespace FleetCarePro.ViewModels
{
    public class ServiceLineItemViewModel
    {
        [Required]
        public int ServiceCategoryId { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a positive value.")]
        public decimal Cost { get; set; }
    }
}